module Jay

open System
open System.Globalization
open System.IO
open System.Text

let trySome fn = try fn |> Some with _ -> None

[<RequireQualifiedAccess>]
type Json =
  | Null  
  | Bool   of bool
  | String of string
  | Number of float  
  | Array  of elements:Json[]
  | Object of properties:(string * Json)[]

module internal StringParser =     
    let parseWith (tryParseFunc: string -> bool * _) = 
        tryParseFunc >> function
        | true, v    -> Some v
        | false, _   -> None
      
    let parseInt16          cul = parseWith (fun str -> Int16.TryParse(str, NumberStyles.Currency, cul))
    let parseInt32          cul = parseWith (fun str -> Int32.TryParse(str, NumberStyles.Currency, cul))
    let parseInt64          cul = parseWith (fun str -> Int64.TryParse(str, NumberStyles.Currency, cul))
    let parseInt            cul = parseInt32 cul
    let parseFloat          cul = parseWith (fun str -> Double.TryParse(str, NumberStyles.Currency, cul))
    let parseDecimal        cul = parseWith (fun str -> Decimal.TryParse(str, NumberStyles.Currency, cul))
    let parseDateTime       cul = parseWith (fun str -> DateTime.TryParse(str, cul, DateTimeStyles.AllowWhiteSpaces ||| DateTimeStyles.RoundtripKind))
    let parseDateTimeOffset cul = parseWith (fun str -> DateTimeOffset.TryParse(str, cul, DateTimeStyles.AllowWhiteSpaces ||| DateTimeStyles.RoundtripKind))
    let parseTimeSpan       cul = parseWith (fun str-> TimeSpan.TryParse(str, cul))
    let parseBoolean            = parseWith Boolean.TryParse
    let parseGuid               = parseWith Guid.TryParse

module internal UnicodeHelper =
    // used http://en.wikipedia.org/wiki/UTF-16#Code_points_U.2B010000_to_U.2B10FFFF as a guide below
    let getUnicodeSurrogatePair num =
        // only code points U+010000 to U+10FFFF supported
        // for coversion to UTF16 surrogate pair
        let codePoint      = num - 0x010000u
        let highTenBitMask = 0xFFC00u                     // 1111|1111|1100|0000|0000
        let lowTenBitMask  = 0x003FFu                      // 0000|0000|0011|1111|1111
        let leadSurrogate  = (codePoint &&& highTenBitMask >>> 10) + 0xD800u
        let trailSurrogate = (codePoint &&& lowTenBitMask) + 0xDC00u
        
        char leadSurrogate, char trailSurrogate

type private JsonParser(jsonText:string) =
    let mutable i = 0
    let s = jsonText
    
    let buf = StringBuilder() // pre-allocate buffers for strings

    // Helper functions
    let skipWhitespace() =
      while i < s.Length && Char.IsWhiteSpace s.[i] do
        i <- i + 1
    
    let isNumChar c =
      Char.IsDigit c || c = '.' || c='e' || c='E' || c='+' || c='-'
    
    let throw() =
      let msg =
        sprintf
          "Invalid JSON starting at character %d, snippet = \n----\n%s\n-----\njson = \n------\n%s\n-------" 
          i (jsonText.[(max 0 (i-10))..(min (jsonText.Length-1) (i+10))]) (if jsonText.Length > 1000 then jsonText.Substring(0, 1000) else jsonText)
      failwith msg
    
    let ensure cond =
      if not cond then throw()

    // Recursive descent parser for JSON that uses global mutable index
    let rec parseValue() =
        skipWhitespace()
        ensure(i < s.Length)
        match s.[i] with
        | '"' -> Json.String(parseString())
        | '-' -> parseNum()
        | c when Char.IsDigit(c) -> parseNum()
        | '{' -> parseObject()
        | '[' -> parseArray()
        | 't' -> parseLiteral("true", Json.Bool true)
        | 'f' -> parseLiteral("false", Json.Bool false)
        | 'n' -> parseLiteral("null", Json.Null)
        | _ -> throw()

    and parseString() =
        ensure(i < s.Length && s.[i] = '"')
        i <- i + 1
        while i < s.Length && s.[i] <> '"' do
            if s.[i] = '\\' then
                ensure(i+1 < s.Length)
                match s.[i+1] with
                | 'b' -> buf.Append('\b') |> ignore
                | 'f' -> buf.Append('\f') |> ignore
                | 'n' -> buf.Append('\n') |> ignore
                | 't' -> buf.Append('\t') |> ignore
                | 'r' -> buf.Append('\r') |> ignore
                | '\\' -> buf.Append('\\') |> ignore
                | '/' -> buf.Append('/') |> ignore
                | '"' -> buf.Append('"') |> ignore
                | 'u' ->
                    ensure(i+5 < s.Length)
                    let hexdigit d =
                        if d >= '0' && d <= '9' then int32 d - int32 '0'
                        elif d >= 'a' && d <= 'f' then int32 d - int32 'a' + 10
                        elif d >= 'A' && d <= 'F' then int32 d - int32 'A' + 10
                        else failwith "hexdigit"
                    let unicodeChar (s:string) =
                        if s.Length <> 4 then failwith "unicodeChar";
                        char (hexdigit s.[0] * 4096 + hexdigit s.[1] * 256 + hexdigit s.[2] * 16 + hexdigit s.[3])
                    let ch = unicodeChar (s.Substring(i+2, 4))
                    buf.Append(ch) |> ignore
                    i <- i + 4  // the \ and u will also be skipped past further below
                | 'U' ->
                    ensure(i+9 < s.Length)
                    let unicodeChar (s:string) =
                        if s.Length <> 8 then failwith "unicodeChar";
                        if s.[0..1] <> "00" then failwith "unicodeChar";
                        UnicodeHelper.getUnicodeSurrogatePair <| UInt32.Parse(s, NumberStyles.HexNumber) 
                    let lead, trail = unicodeChar (s.Substring(i+2, 8))
                    buf.Append(lead) |> ignore
                    buf.Append(trail) |> ignore
                    i <- i + 8  // the \ and u will also be skipped past further below
                | _ -> throw()
                i <- i + 2  // skip past \ and next char
            else
                buf.Append(s.[i]) |> ignore
                i <- i + 1
        ensure(i < s.Length && s.[i] = '"')
        i <- i + 1
        let str = buf.ToString()
        buf.Clear() |> ignore
        str

    and parseNum() =
        let start = i
        while i < s.Length && (isNumChar s.[i]) do
            i <- i + 1
        let len = i - start
        let sub = s.Substring(start,len)
        match StringParser.parseFloat CultureInfo.InvariantCulture sub with
        | Some x -> Json.Number x
        | _      -> throw()

    and parsePair() =
        let key = parseString()
        skipWhitespace()
        ensure(i < s.Length && s.[i] = ':')
        i <- i + 1
        skipWhitespace()
        key, parseValue()

    and parseObject() =
        ensure(i < s.Length && s.[i] = '{')
        i <- i + 1
        skipWhitespace()
        let pairs = ResizeArray<_>()
        if i < s.Length && s.[i] = '"' then
            pairs.Add(parsePair())
            skipWhitespace()
            while i < s.Length && s.[i] = ',' do
                i <- i + 1
                skipWhitespace()
                pairs.Add(parsePair())
                skipWhitespace()
        ensure(i < s.Length && s.[i] = '}')
        i <- i + 1
        Json.Object(pairs.ToArray())

    and parseArray() =
        ensure(i < s.Length && s.[i] = '[')
        i <- i + 1
        skipWhitespace()
        let vals = ResizeArray<_>()
        if i < s.Length && s.[i] <> ']' then
            vals.Add(parseValue())
            skipWhitespace()
            while i < s.Length && s.[i] = ',' do
                i <- i + 1
                skipWhitespace()
                vals.Add(parseValue())
                skipWhitespace()
        ensure(i < s.Length && s.[i] = ']')
        i <- i + 1
        Json.Array(vals.ToArray())

    and parseLiteral(expected, r) =
        ensure(i+expected.Length <= s.Length)
        for j in 0 .. expected.Length - 1 do
            ensure(s.[i+j] = expected.[j])
        i <- i + expected.Length
        r

    // Start by parsing the top-level value
    member _.Parse() =
        let value = parseValue()
        skipWhitespace()
        if i <> s.Length then
            throw()
        value

module Json =
    let parse (str : string) = 
        JsonParser(str).Parse()
   
    let parseStream (str : Stream) =
        use reader = new StreamReader(str)
        let text = reader.ReadToEnd()
        JsonParser(text).Parse()

    let serialize (json : Json) =    
        let propSep = "\":"

        // Encode characters that are not valid in JS string. The implementation is based
        // on https://github.com/mono/mono/blob/master/mcs/class/System.Web/System.Web/HttpUtility.cs
        let jsonEncode (w : TextWriter) (value : string) =
          if not (String.IsNullOrEmpty value) then
            for i = 0 to value.Length - 1 do
              let c = value.[i]
              let ci = int c
              if ci >= 0 && ci <= 7 || ci = 11 || ci >= 14 && ci <= 31 then
                w.Write("\\u{0:x4}", ci) |> ignore
              else 
                match c with
                | '\b' -> w.Write "\\b"
                | '\t' -> w.Write "\\t"
                | '\n' -> w.Write "\\n"
                | '\f' -> w.Write "\\f"
                | '\r' -> w.Write "\\r"
                | '"'  -> w.Write "\\\""
                | '\\' -> w.Write "\\\\"
                | _    -> w.Write c

        let rec serializeJson (w : TextWriter) (json : Json) = 
            match json with
            | Json.Null -> w.Write "null"
            | Json.Bool b -> w.Write(if b then "true" else "false")
            | Json.Number number -> w.Write number            
            | Json.String s ->
                w.Write "\""
                jsonEncode w s
                w.Write "\""
            | Json.Object properties ->
                w.Write "{"                      
                for i = 0 to properties.Length - 1 do
                    let k,v = properties.[i]
                    if i > 0 then w.Write ","                
                    w.Write "\""
                    jsonEncode w k
                    w.Write propSep
                    serializeJson w v
                w.Write "}"
            | Json.Array elements ->
                w.Write "["
                for i = 0 to elements.Length - 1 do
                    if i > 0 then w.Write ","                
                    serializeJson w elements.[i]
                w.Write "]"
      
        let w = new StringWriter()
        serializeJson w json
        w.GetStringBuilder().ToString()
       
    let tryParse (str : string) = 
        trySome (JsonParser(str).Parse())

    let tryParseStream (str : Stream) =
        trySome (fun _ -> 
            use reader = new StreamReader(str)
            let text = reader.ReadToEnd()
            JsonParser(text).Parse())
    
module internal JsonConvert =     
    let inline floatInRange min max (f : float) = 
        let _min = float min
        let _max = float max
        f >= _min && f <= _max

    let asString (cul : CultureInfo) (json : Json) =
        match json with 
        | Json.Null          -> Some ""
        | Json.Bool b when b -> Some (if b then "true" else "false")
        | Json.String s      -> Some s
        | Json.Number n      -> Some (n.ToString(cul))
        | _                  -> None
  
    let asInt16 (cul : CultureInfo) (json : Json) =        
        match json with
        | Json.Number n when floatInRange Int16.MinValue Int16.MaxValue n -> Some (Convert.ToInt16(n))            
        | Json.String s -> StringParser.parseInt16 cul s
        | _             -> None

    let asInt32 (cul : CultureInfo) (json : Json) =        
        match json with
        | Json.Number n when floatInRange Int32.MinValue Int32.MaxValue n -> Some (Convert.ToInt32(n))            
        | Json.String s -> StringParser.parseInt32 cul s
        | _             -> None

    let asInt64 (cul : CultureInfo) (json : Json) =        
        match json with
        | Json.Number n when floatInRange Int64.MinValue Int64.MaxValue n -> Some (Convert.ToInt64(n))
        | Json.String s -> StringParser.parseInt64 cul s
        | _             -> None

    let asInt (cul : CultureInfo) (json : Json) = asInt32 cul json

    let asBool (json : Json) =
        match json with 
        | Json.Bool b     -> Some b
        | Json.Number 1.0 -> Some true
        | Json.Number 0.0 -> Some false
        | Json.String s   -> StringParser.parseBoolean s
        | _               -> None

    let asFloat (cul : CultureInfo) (json : Json) = 
        match json with
        | Json.Number n -> Some (float n)
        | Json.String s -> StringParser.parseFloat cul s
        | _             -> None

    let asDecimal (cul : CultureInfo) (json : Json) =
        match json with
        | Json.Number n -> Some (decimal n)
        | Json.String s -> StringParser.parseDecimal cul s
        | _             -> None

    let epoch = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)

    let asDateTime (cul : CultureInfo) (json : Json) =
        match json with 
        | Json.Number n when floatInRange Int64.MinValue Int64.MaxValue n -> 
            Some (epoch.AddMilliseconds(float n))
        | Json.String s -> StringParser.parseDateTime cul s
        | _             -> None

    let asDateTimeOffset (cul : CultureInfo) (json : Json) =
        match json with
        | Json.Number n when floatInRange Int64.MinValue Int64.MaxValue n -> 
            Some (DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(n)))
        | Json.String s -> StringParser.parseDateTimeOffset cul s
        | _             -> None

    let asTimeSpan (cul : CultureInfo) (json : Json) =
        match json with 
        | Json.String s -> StringParser.parseTimeSpan cul s
        | _             -> None

    let asGuid (json : Json) = 
        match json with 
        | Json.String s -> StringParser.parseGuid s
        | _             -> None
            
type Json with        
    member this.AsArray () =
        match this with 
        | Json.Array a -> a
        | _            -> [||]

    member inline this.Item(i : int) = 
        this.AsArray().[i]

    member this.TryGet (name : string) =
        match this with 
        | Json.Object props -> 
            Array.tryFind (fst >> (=) name) props |> Option.map snd
        | _ -> None            

    member this.Get (name : string) =
        match this.TryGet name with
        | Some prop -> prop
        | None      -> failwithf "Property %s does not exist" name

   
    member this.TryStringFormat (cul : CultureInfo) =
        JsonConvert.asString cul this

    member this.AsStringFormat (cul : CultureInfo) =
        match this.TryStringFormat cul with
        | Some s -> s
        | None   -> failwithf "%s is not a string" (Json.serialize this)
        
    member this.TryString () =
        this.TryStringFormat CultureInfo.InvariantCulture

    member this.AsString () =
        this.AsStringFormat CultureInfo.InvariantCulture        

    member this.AsInt16 () =
        let cul = CultureInfo.InvariantCulture
        match JsonConvert.asInt16 cul this with
        | Some s -> s
        | None   -> failwithf "%s is not an Int32" (Json.serialize this)

    member this.AsInt32 () =
        let cul = CultureInfo.InvariantCulture
        match JsonConvert.asInt32 cul this with
        | Some s -> s
        | None   -> failwithf "%s is not an Int32" (Json.serialize this)

    member this.AsInt64 () =
        let cul = CultureInfo.InvariantCulture
        match JsonConvert.asInt64 cul this with
        | Some s -> s
        | None   -> failwithf "%s is not an Int64" (Json.serialize this)

    member this.AsInt () = this.AsInt32 ()

    member this.AsBool () =                
        match JsonConvert.asBool this with
        | Some s -> s
        | None   -> failwithf "%s is not a Boolean" (Json.serialize this)

    member this.AsFloat () =
        let cul = CultureInfo.InvariantCulture
        match JsonConvert.asFloat cul this with
        | Some s -> s
        | None   -> failwithf "%s is not an Float" (Json.serialize this)

    member this.AsDecimal () =
        let cul = CultureInfo.InvariantCulture
        match JsonConvert.asDecimal cul this with
        | Some d -> d
        | None   -> failwithf "%s is not a Decimal" (Json.serialize this)

    member this.AsDateTime () =
        let cul = CultureInfo.InvariantCulture
        match JsonConvert.asDateTime cul this with
        | Some d -> d
        | None   -> failwithf "%s is not a DateTime" (Json.serialize this)

    member this.AsDateTimeOffset () =
        let cul = CultureInfo.InvariantCulture
        match JsonConvert.asDateTimeOffset cul this with
        | Some d -> d
        | None   -> failwithf "%s is not a DateTimeOffset" (Json.serialize this)

    member this.AsTimeSpan () =
        let cul = CultureInfo.InvariantCulture
        match JsonConvert.asTimeSpan cul this with
        | Some t -> t
        | None   -> failwithf "%s is not a TimeSpan" (Json.serialize this)

    member this.AsGuid () =        
        match JsonConvert.asGuid this with
        | Some t -> t
        | None   -> failwithf "%s is not a Guid" (Json.serialize this)

let (?) (json : Json) (name : string) =
    json.Get name
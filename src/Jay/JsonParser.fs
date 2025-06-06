﻿[<AutoOpen>]
module Jay.JsonParser

open System
open System.Globalization
open System.IO
open System.Text

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

type internal JsonParser(jsonText:string) =
    let mutable i = 0
    let s = jsonText
    
    let buf = StringBuilder() // pre-allocate buffers for strings

    // Helper functions
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

    let rec skipCommentsAndWhitespace () =
        let skipComment () =
            // Supported comment syntax:
            // - // ...{newLine}
            // - /* ... */
            if i < s.Length && s.[i] = '/' then
                i <- i + 1

                if i < s.Length && s.[i] = '/' then
                    i <- i + 1
                    while i < s.Length && (s.[i] <> '\r' && s.[i] <> '\n') do
                        i <- i + 1
                else if i < s.Length && s.[i] = '*' then
                    i <- i + 1
                    while i + 1 < s.Length && s.[i] <> '*' && s.[i + 1] <> '/' do
                        i <- i + 1
                    ensure (i + 1 < s.Length && s.[i] = '*' && s.[i + 1] = '/')
                    i <- i + 2
                true

            else
                false

        let skipWhitespace () =
            let initialI = i
            while i < s.Length && Char.IsWhiteSpace s.[i] do
                i <- i + 1
            initialI <> i // return true if some whitespace was skipped

        if skipWhitespace () || skipComment () then
            skipCommentsAndWhitespace ()

    // Recursive descent parser for JSON that uses global mutable index
    let rec parseValue() =
        skipCommentsAndWhitespace()
        ensure(i < s.Length)
        match s.[i] with
        | '"' -> JString(parseString())
        | '-' -> parseNum()
        | c when Char.IsDigit(c) -> parseNum()
        | '{' -> parseObject()
        | '[' -> parseArray()
        | 't' -> parseLiteral("true", JBool true)
        | 'f' -> parseLiteral("false", JBool false)
        | 'n' -> parseLiteral("null", JNull)
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
        | Some x -> JNumber x
        | _      -> throw()

    and parsePair() =
        let key = parseString()
        skipCommentsAndWhitespace()
        ensure(i < s.Length && s.[i] = ':')
        i <- i + 1
        skipCommentsAndWhitespace()
        key, parseValue()

    and parseObject() =
        ensure(i < s.Length && s.[i] = '{')
        i <- i + 1
        skipCommentsAndWhitespace()
        let pairs = ResizeArray<_>()
        if i < s.Length && s.[i] = '"' then
            pairs.Add(parsePair())
            skipCommentsAndWhitespace()
            while i < s.Length && s.[i] = ',' do
                i <- i + 1
                skipCommentsAndWhitespace()
                pairs.Add(parsePair())
                skipCommentsAndWhitespace()
        ensure(i < s.Length && s.[i] = '}')
        i <- i + 1
        JObject(pairs.ToArray())

    and parseArray() =
        ensure(i < s.Length && s.[i] = '[')
        i <- i + 1
        skipCommentsAndWhitespace()
        let vals = ResizeArray<_>()
        if i < s.Length && s.[i] <> ']' then
            vals.Add(parseValue())
            skipCommentsAndWhitespace()
            while i < s.Length && s.[i] = ',' do
                i <- i + 1
                skipCommentsAndWhitespace()
                vals.Add(parseValue())
                skipCommentsAndWhitespace()
        ensure(i < s.Length && s.[i] = ']')
        i <- i + 1
        JArray(vals.ToArray())

    and parseLiteral(expected, r) =
        ensure(i+expected.Length <= s.Length)
        for j in 0 .. expected.Length - 1 do
            ensure(s.[i+j] = expected.[j])
        i <- i + expected.Length
        r

    // Start by parsing the top-level value
    member _.Parse() =
        let value = parseValue()
        skipCommentsAndWhitespace()
        if i <> s.Length then
            throw()
        value
[<AutoOpen>]
module Jay.Json

open System
open System.IO

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
            | JNull -> w.Write "null"
            | JBool b -> w.Write(if b then "true" else "false")
            | JNumber number -> w.Write number            
            | JString s ->
                w.Write "\""
                jsonEncode w s
                w.Write "\""
            | JObject properties ->
                w.Write "{"                      
                for i = 0 to properties.Length - 1 do
                    let k,v = properties.[i]
                    if i > 0 then w.Write ","                
                    w.Write "\""
                    jsonEncode w k
                    w.Write propSep
                    serializeJson w v
                w.Write "}"
            | JArray elements ->
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
        use reader = new StreamReader(str)
        let text = reader.ReadToEnd()
        trySome (JsonParser(text).Parse())
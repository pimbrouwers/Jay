[<AutoOpen>]
module Jay.JsonExtensions

open System
open System.Globalization
open System.Runtime.CompilerServices 
open JsonConvert

[<Extension>]
type JsonExtensions() =    
    [<Extension>]
    static member AsArray (this : Json) : Json[] =
        match this with 
        | JArray a -> a
        | _            -> [||]

    [<Extension>]
    static member inline Item (this : Json, i : int) : Json = 
        this.AsArray().[i]
    
    [<Extension>]
    static member TryGet (this : Json, name : string) : Json option =
        match this with 
        | JObject props -> 
            Array.tryFind (fst >> (=) name) props |> Option.map snd
        | _ -> None            
    
    [<Extension>]
    static member Get (this : Json, name : string) : Json =                
        match this.TryGet name with
        | Some prop -> prop
        | None      -> failwithf "Property %s does not exist" name
           
    [<Extension>]
    static member TryStringFormat (this : Json, cul : CultureInfo) : string option =
        asString cul this
    
    [<Extension>]
    static member AsStringFormat (this : Json, cul : CultureInfo) : string =
        match this.TryStringFormat cul with
        | Some s -> s
        | None   -> failwithf "%s is not a string" (Json.serialize this)
        
    [<Extension>]
    static member TryString (this : Json) : string option =
        this.TryStringFormat CultureInfo.InvariantCulture
    
    [<Extension>]
    static member AsString (this : Json) : string =
        this.AsStringFormat CultureInfo.InvariantCulture        
    
    [<Extension>]
    static member AsInt16 (this : Json) : int16 =
        convertOrFail "Int16" asInt16 this
    
    [<Extension>]
    static member AsInt32 (this : Json) : int32  =    
        convertOrFail "Int32" asInt32 this
    
    [<Extension>]
    static member AsInt64 (this : Json) : int64 =
        convertOrFail "Int64" asInt64 this
    
    [<Extension>]
    static member AsInt (this : Json) : int32 = this.AsInt32 ()
    
    [<Extension>]
    static member AsBool (this : Json) : bool =   
        convertOrFail "Bool" (fun _ json -> asBool json) this
    
    [<Extension>]
    static member AsFloat (this : Json) : float =
        convertOrFail "Float" asFloat this
    
    [<Extension>]
    static member AsDecimal (this : Json) : decimal =
        convertOrFail "Decimal" asDecimal this
    
    [<Extension>]
    static member AsDateTime (this : Json) : DateTime =
        convertOrFail "DateTime" asDateTime this
    
    [<Extension>]
    static member AsDateTimeOffset (this : Json) : DateTimeOffset =
        convertOrFail "DateTimeOffset" asDateTimeOffset this
    
    [<Extension>]
    static member AsTimeSpan (this : Json) : TimeSpan =
        convertOrFail "TimeSpan" asTimeSpan this
    
    [<Extension>]
    static member AsGuid (this : Json) : Guid =
        convertOrFail "Guid" (fun _ json -> asGuid json) this
    
let (?) (json : Json) (name : string) : Json =
    json.Get name
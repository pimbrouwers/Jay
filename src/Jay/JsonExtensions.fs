[<AutoOpen>]
module Jay.JsonExtensions

open System
open System.Globalization
open System.Runtime.CompilerServices 
open JsonConvert

[<Extension>]
type JsonExtensions() =    
    [<Extension>]
    static member AsPropertyArray (this : Json) =
        match this with
        | JObject properties -> properties
        | _                  -> [||]

    [<Extension>]
    static member AsPropertyArrayOrNone (this : Json, name : string) =
        match this.TryGet name with
        | Some prop -> prop.AsPropertyArray() |> Some
        | None      -> None

    [<Extension>]
    static member AsArray (this : Json) : Json[] =
        match this with 
        | JArray a -> a
        | _        -> [||]

    [<Extension>]
    static member AsArrayOrNone (this : Json, name : string) : Json[] option =
        match this.TryGet name with
        | Some prop -> prop.AsArray() |> Some
        | None      -> None

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
    static member AsString (this : Json) : string =
        this.AsStringFormat CultureInfo.InvariantCulture 

    [<Extension>]
    static member AsStringOrNone (this : Json, name : string) : string option =
        match this.TryGet name with
        | Some prop -> prop.AsString() |> Some
        | None      -> None
    
    [<Extension>]
    static member AsInt16 (this : Json) : int16 =
        convertOrFail "Int16" asInt16 this

    [<Extension>]
    static member AsInt16OrNone (this : Json, name : string) : int16 option =
        match this.TryGet name with
        | Some prop -> convertOrFail "Int16" asInt16 prop |> Some
        | None      -> None
    
    [<Extension>]
    static member AsInt32 (this : Json) : int32  =    
        convertOrFail "Int32" asInt32 this

    [<Extension>]
    static member AsInt32OrNone (this : Json, name : string) : int32 option =
        match this.TryGet name with
        | Some prop -> convertOrFail "Int32" asInt32 prop |> Some
        | None      -> None
     
    [<Extension>]
    static member AsInt64 (this : Json) : int64 =
        convertOrFail "Int64" asInt64 this

    [<Extension>]
    static member AsInt64OrNone (this : Json, name : string) : int64 option =
        match this.TryGet name with
        | Some prop -> convertOrFail "Int64" asInt64 prop |> Some
        | None      -> None
         
    [<Extension>]
    static member AsInt (this : Json) : int32 = this.AsInt32 ()

    [<Extension>]
    static member AsIntOrNone (this : Json, name : string) : int option =
        match this.TryGet name with
        | Some prop -> prop.AsInt32 () |> Some
        | None      -> None
    
    [<Extension>]
    static member AsBool (this : Json) : bool =   
        convertOrFail "Bool" (fun _ json -> asBool json) this

    [<Extension>]
    static member AsBoolOrNone (this : Json, name : string) : bool option =
        match this.TryGet name with
        | Some prop -> convertOrFail "Bool" (fun _ json -> asBool json) prop |> Some
        | None      -> None
    
    [<Extension>]
    static member AsFloat (this : Json) : float =
        convertOrFail "Float" asFloat this

    [<Extension>]
    static member AsFloatOrNone (this : Json, name : string) : float option =
        match this.TryGet name with
        | Some prop -> convertOrFail "Float" asFloat prop |> Some
        | None      -> None
    
    [<Extension>]
    static member AsDecimal (this : Json) : decimal =
        convertOrFail "Decimal" asDecimal this

    [<Extension>]
    static member AsDecimalOrNone (this : Json, name : string) : decimal option =
        match this.TryGet name with
        | Some prop -> convertOrFail "Decimal" asDecimal prop |> Some
        | None      -> None
    
    [<Extension>]
    static member AsDateTime (this : Json) : DateTime =
        convertOrFail "DateTime" asDateTime this
    
    [<Extension>]
    static member AsDateTimeOrNone (this : Json, name : string) : DateTime option =
        match this.TryGet name with
        | Some prop -> convertOrFail "DateTime" asDateTime prop |> Some
        | None      -> None
    
    [<Extension>]
    static member AsDateTimeOffset (this : Json) : DateTimeOffset =
        convertOrFail "DateTimeOffset" asDateTimeOffset this
        
    [<Extension>]
    static member AsDateTimeOffsetOrNone (this : Json, name : string) : DateTimeOffset option =
        match this.TryGet name with
        | Some prop -> convertOrFail "DateTimeOffset" asDateTimeOffset prop |> Some
        | None      -> None
    
    [<Extension>]
    static member AsTimeSpan (this : Json) : TimeSpan =
        convertOrFail "TimeSpan" asTimeSpan this

    [<Extension>]
    static member AsTimeSpanOrNone (this : Json, name : string) : TimeSpan option =
        match this.TryGet name with
        | Some prop -> convertOrFail "TimeSpan" asTimeSpan prop |> Some
        | None      -> None
    
    [<Extension>]
    static member AsGuid (this : Json) : Guid =
        convertOrFail "Guid" (fun _ json -> asGuid json) this

    [<Extension>]
    static member AsGuidOrNone (this : Json, name : string) : Guid option =
        match this.TryGet name with
        | Some prop -> convertOrFail "Guid" (fun _ json -> asGuid json) prop |> Some
        | None      -> None
    
let (?) (json : Json) (name : string) : Json =
    json.Get name

    
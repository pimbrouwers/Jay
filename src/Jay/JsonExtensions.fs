[<AutoOpen>]
module Jay.JsonExtensions

open System
open System.Globalization
open System.Runtime.CompilerServices
open JsonConvert

[<Extension>]
type JsonExtensions() =

    [<Extension>]
    static member TryGet(this: Json, name: string) : Json option =
        match this with
        | JObject props ->
            Array.tryFind (fst >> (=) name) props
            |> Option.map snd
        | _ -> None

    [<Extension>]
    static member TryGet(this: Json option, name: string) : Json option =
        match this with
        | Some json ->
            match json with
            | JObject props ->
                Array.tryFind (fst >> (=) name) props
                |> Option.map snd
            | _ -> None
        | _ -> None

    [<Extension>]
    static member Get(this: Json, name: string) : Json =
        match this.TryGet(name) with
        | Some prop -> prop
        | None -> raise (PropertyNotFoundException $"Property '{name}' does not exist in JSON")

    [<Extension>]
    static member Get(this: Json option, name: string) : Json =
        match this with
        | Some json ->
            match json.TryGet(name) with
            | Some prop -> prop
            | None -> raise (PropertyNotFoundException $"Property '{name}' does not exist in JSON")
        | None -> raise (PropertyNotFoundException $"Property '{name}' does not exist in JSON")

    [<Extension>]
    static member AsPropertyArray(this: Json) =
        match this with
        | JObject properties -> properties
        | _ -> raise (InvalidPropertyTypeException $"Expected an 'object/map' in JSON. Got - {Json.serialize (this)}")

    [<Extension>]
    static member AsPropertyArrayOrNone(this: Json) =
        match this with
        | JObject properties -> Some properties
        | JNull -> None
        | _ -> raise (InvalidPropertyTypeException $"Expected an 'object/map' in JSON. Got - {Json.serialize (this)}")


    [<Extension>]
    static member AsPropertyArrayOrNone(this: Json option) =
        match this with
        | Some prop ->
            match prop with
            | JObject properties -> Some properties
            | JNull -> None
            | _ ->
                raise (InvalidPropertyTypeException $"Expected an 'object/map' in JSON. Got - {Json.serialize (prop)}")


        | None -> None

    [<Extension>]
    static member AsArray(this: Json) : Json [] =
        match this with
        | JArray a -> a
        | _ -> raise (InvalidPropertyTypeException $"Expected an 'array' in JSON. Got - {Json.serialize (this)}")

    [<Extension>]
    static member AsArrayOrNone(this: Json) : Json [] option =
        match this with
        | JArray a -> Some a
        | JNull -> None
        | _ -> raise (InvalidPropertyTypeException $"Expected an 'array' in JSON. Got - {Json.serialize (this)}")


    [<Extension>]
    static member AsArrayOrNone(this: Json option) : Json [] option =
        match this with
        | Some prop ->
            match prop with
            | JArray a -> Some a
            | JNull -> None
            | _ -> raise (InvalidPropertyTypeException $"Expected an 'array' in JSON. Got - {Json.serialize (prop)}")

        | None -> None

    [<Extension>]
    static member inline Item(this: Json, i: int) : Json = this.AsArray().[i]

    [<Extension>]
    static member TryStringFormat(this: Json, cul: CultureInfo) : string option = asString cul this

    [<Extension>]
    static member AsStringFormat(this: Json, cul: CultureInfo) : string =
        match this.TryStringFormat cul with
        | Some s -> s
        | None -> raise (InvalidPropertyTypeException $"Expected a 'string' in JSON. Got - {Json.serialize (this)}")

    [<Extension>]
    static member AsString(this: Json) : string =
        this.AsStringFormat CultureInfo.InvariantCulture

    [<Extension>]
    static member AsStringOrNone(this: Json) : string option =
        this.TryStringFormat CultureInfo.InvariantCulture

    [<Extension>]
    static member AsStringOrNone(this: Json option) : string option =
        match this with
        | Some prop -> prop.AsStringOrNone()
        | None -> None

    [<Extension>]
    static member AsInt16(this: Json) : int16 = convertOrFail "Int16" asInt16 this

    [<Extension>]
    static member AsInt16OrNone(this: Json) : int16 option = convertOrNone asInt16 this

    [<Extension>]
    static member AsInt16OrNone(this: Json option) : int16 option =
        match this with
        | Some prop -> convertOrNone asInt16 prop
        | None -> None

    [<Extension>]
    static member AsInt32(this: Json) : int32 = convertOrFail "Int32" asInt32 this

    [<Extension>]
    static member AsInt32OrNone(this: Json) : int32 option = convertOrNone asInt32 this

    [<Extension>]
    static member AsInt32OrNone(this: Json option) : int32 option =
        match this with
        | Some prop -> convertOrNone asInt32 prop
        | None -> None

    [<Extension>]
    static member AsInt64(this: Json) : int64 = convertOrFail "Int64" asInt64 this

    [<Extension>]
    static member AsInt64OrNone(this: Json) : int64 option = convertOrNone asInt64 this

    [<Extension>]
    static member AsInt64OrNone(this: Json option) : int64 option =
        match this with
        | Some prop -> convertOrNone asInt64 prop
        | None -> None

    [<Extension>]
    static member AsInt(this: Json) : int32 = this.AsInt32()

    [<Extension>]
    static member AsIntOrNone(this: Json) : int32 option = this.AsInt32OrNone()

    [<Extension>]
    static member AsIntOrNone(this: Json option) : int32 option = this.AsInt32OrNone()

    [<Extension>]
    static member AsBool(this: Json) : bool =
        convertOrFail "Bool" (fun _ json -> asBool json) this

    [<Extension>]
    static member AsBoolOrNone(this: Json) : bool option =
        convertOrNone (fun _ json -> asBool json) this

    [<Extension>]
    static member AsBoolOrNone(this: Json option) : bool option =
        match this with
        | Some prop -> convertOrNone (fun _ json -> asBool json) prop
        | None -> None

    [<Extension>]
    static member AsFloat(this: Json) : float = convertOrFail "Float" asFloat this

    [<Extension>]
    static member AsFloatOrNone(this: Json) : float option = convertOrNone asFloat this

    [<Extension>]
    static member AsFloatOrNone(this: Json option) : float option =
        match this with
        | Some prop -> convertOrNone asFloat prop
        | None -> None

    [<Extension>]
    static member AsDecimal(this: Json) : decimal = convertOrFail "Decimal" asDecimal this

    [<Extension>]
    static member AsDecimalOrNone(this: Json) : decimal option = convertOrNone asDecimal this

    [<Extension>]
    static member AsDecimalOrNone(this: Json option) : decimal option =
        match this with
        | Some prop -> convertOrNone asDecimal prop
        | None -> None

    [<Extension>]
    static member AsDateTime(this: Json) : DateTime =
        convertOrFail "DateTime" asDateTime this

    [<Extension>]
    static member AsDateTimeOrNone(this: Json) : DateTime option = convertOrNone asDateTime this

    [<Extension>]
    static member AsDateTimeOrNone(this: Json option) : DateTime option =
        match this with
        | Some prop -> convertOrNone asDateTime prop
        | None -> None

    [<Extension>]
    static member AsDateTimeOffset(this: Json) : DateTimeOffset =
        convertOrFail "DateTimeOffset" asDateTimeOffset this

    [<Extension>]
    static member AsDateTimeOffsetOrNone(this: Json) : DateTimeOffset option = convertOrNone asDateTimeOffset this

    [<Extension>]
    static member AsDateTimeOffsetOrNone(this: Json option) : DateTimeOffset option =
        match this with
        | Some prop -> convertOrNone asDateTimeOffset prop
        | None -> None

    [<Extension>]
    static member AsTimeSpan(this: Json) : TimeSpan =
        convertOrFail "TimeSpan" asTimeSpan this

    [<Extension>]
    static member AsTimeSpanOrNone(this: Json) : TimeSpan option = convertOrNone asTimeSpan this

    [<Extension>]
    static member AsTimeSpanOrNone(this: Json option) : TimeSpan option =
        match this with
        | Some prop -> convertOrNone asTimeSpan prop
        | None -> None

    [<Extension>]
    static member AsGuid(this: Json) : Guid =
        convertOrFail "Guid" (fun _ json -> asGuid json) this

    [<Extension>]
    static member AsGuidOrNone(this: Json) : Guid option =
        convertOrNone (fun _ json -> asGuid json) this

    [<Extension>]
    static member AsGuidOrNone(this: Json option) : Guid option =
        match this with
        | Some prop -> convertOrNone (fun _ json -> asGuid json) prop
        | None -> None


//Pipeline functions for convenience
module Json =
    let get (property: string) (json: Json) : Json = json.Get(property)

    let tryGet (property: string) (json: Json) : Json option = json.TryGet(property)

    module Optional =
        let get (property: string) (json: Json option) : Json option =
            match json with
            | Some j -> j.Get(property) |> Some
            | None -> None

        let tryGet (property: string) (json: Json option) : Json option =
            match json with
            | Some j -> j.TryGet(property)
            | None -> None

//JSON path operators for convenience

let (?) (json: Json) (name: string) : Json = json.Get(name)

//Optional on the both sides
let (<??>) (json: Json option) (name: string) =
    match json with
    | Some jdoc -> jdoc.TryGet(name)
    | None -> None

//Not Optional
let (</>) (json: Json) (name: string) : Json = json.Get(name)

//Optional on the right
let (</?>) (json: Json) (name: string) = json.TryGet(name)

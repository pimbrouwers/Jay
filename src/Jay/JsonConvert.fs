[<AutoOpen>]
module internal Jay.JsonConvert

open System
open System.Globalization

let inline floatInRange min max (f : float) = 
    let _min = float min
    let _max = float max
    f >= _min && f <= _max

let convertOrFail 
    (typeName : string) 
    (ctor : CultureInfo -> Json -> 'a option)
    (this : Json): 'a =
    let cul = CultureInfo.InvariantCulture
    match ctor cul this with
    | Some s -> s
    | None   -> failwithf "%s is not an %s" (Json.serialize this) typeName

let asString (cul : CultureInfo) (json : Json) =
    match json with 
    | JNull          -> Some ""
    | JBool b when b -> Some (if b then "true" else "false")
    | JString s      -> Some s
    | JNumber n      -> Some (n.ToString(cul))
    | _                  -> None
  
let asInt16 (cul : CultureInfo) (json : Json) =        
    match json with
    | JNumber n when floatInRange Int16.MinValue Int16.MaxValue n -> Some (Convert.ToInt16(n))            
    | JString s -> StringParser.parseInt16 cul s
    | _             -> None

let asInt32 (cul : CultureInfo) (json : Json) =        
    match json with
    | JNumber n when floatInRange Int32.MinValue Int32.MaxValue n -> Some (Convert.ToInt32(n))            
    | JString s -> StringParser.parseInt32 cul s
    | _             -> None

let asInt64 (cul : CultureInfo) (json : Json) =        
    match json with
    | JNumber n when floatInRange Int64.MinValue Int64.MaxValue n -> Some (Convert.ToInt64(n))
    | JString s -> StringParser.parseInt64 cul s
    | _             -> None

let asInt (cul : CultureInfo) (json : Json) = asInt32 cul json

let asBool (json : Json) =
    match json with 
    | JBool b     -> Some b
    | JNumber 1.0 -> Some true
    | JNumber 0.0 -> Some false
    | JString s   -> StringParser.parseBoolean s
    | _               -> None

let asFloat (cul : CultureInfo) (json : Json) = 
    match json with
    | JNumber n -> Some (float n)
    | JString s -> StringParser.parseFloat cul s
    | _             -> None

let asDecimal (cul : CultureInfo) (json : Json) =
    match json with
    | JNumber n -> Some (decimal n)
    | JString s -> StringParser.parseDecimal cul s
    | _             -> None

let epoch = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)

let asDateTime (cul : CultureInfo) (json : Json) =
    match json with 
    | JNumber n when floatInRange Int64.MinValue Int64.MaxValue n -> 
        Some (epoch.AddMilliseconds(float n))
    | JString s -> StringParser.parseDateTime cul s
    | _             -> None

let asDateTimeOffset (cul : CultureInfo) (json : Json) =
    match json with
    | JNumber n when floatInRange Int64.MinValue Int64.MaxValue n -> 
        Some (DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(n)))
    | JString s -> StringParser.parseDateTimeOffset cul s
    | _             -> None

let asTimeSpan (cul : CultureInfo) (json : Json) =
    match json with 
    | JString s -> StringParser.parseTimeSpan cul s
    | _             -> None

let asGuid (json : Json) = 
    match json with 
    | JString s -> StringParser.parseGuid s
    | _             -> None


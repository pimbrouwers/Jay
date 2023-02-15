[<AutoOpen>]
module Jay.Common

open System
open System.Globalization

type Json =
    | JNull
    | JBool of bool
    | JString of string
    | JNumber of float
    | JArray of elements: Json []
    | JObject of properties: (string * Json) []

module internal StringParser =
    let parseWith (tryParseFunc: string -> bool * _) =
        tryParseFunc
        >> function
        | true, v -> Some v
        | false, _ -> None

    let parseInt16 cul =
        parseWith (fun str -> Int16.TryParse(str, NumberStyles.Currency, cul))

    let parseInt32 cul =
        parseWith (fun str -> Int32.TryParse(str, NumberStyles.Currency, cul))

    let parseInt64 cul =
        parseWith (fun str -> Int64.TryParse(str, NumberStyles.Currency, cul))

    let parseInt cul = parseInt32 cul

    let parseFloat cul =
        parseWith (fun str -> Double.TryParse(str, NumberStyles.Currency, cul))

    let parseDecimal cul =
        parseWith (fun str -> Decimal.TryParse(str, NumberStyles.Currency, cul))

    let parseDateTime cul =
        parseWith
            (fun str ->
                DateTime.TryParse(
                    str,
                    cul,
                    DateTimeStyles.AllowWhiteSpaces
                    ||| DateTimeStyles.RoundtripKind
                ))

    let parseDateTimeOffset cul =
        parseWith
            (fun str ->
                DateTimeOffset.TryParse(
                    str,
                    cul,
                    DateTimeStyles.AllowWhiteSpaces
                    ||| DateTimeStyles.RoundtripKind
                ))

    let parseTimeSpan cul =
        parseWith (fun str -> TimeSpan.TryParse(str, cul))

    let parseBoolean = parseWith Boolean.TryParse
    let parseGuid = parseWith Guid.TryParse

[<AutoOpen>]
module internal SpanParser =
    let inline toOption (parsedResult: bool * _) =
        match parsedResult with
        | true, v -> Some v
        | false, _ -> None

    let parseInt16 (cul: CultureInfo) (spn: ReadOnlySpan<char>) =
        Int16.TryParse(spn, NumberStyles.Currency, CultureInfo.InvariantCulture)
        |> toOption

    let parseInt32 (cul: CultureInfo) (spn: ReadOnlySpan<char>) =
        Int32.TryParse(spn, NumberStyles.Currency, CultureInfo.InvariantCulture)
        |> toOption

    let parseUInt32 (cul: CultureInfo) (spn: ReadOnlySpan<char>) =
        UInt32.Parse(spn, NumberStyles.Currency, CultureInfo.InvariantCulture)

    let parseInt64 (cul: CultureInfo) (spn: ReadOnlySpan<char>) =
        Int64.TryParse(spn, NumberStyles.Currency, CultureInfo.InvariantCulture)
        |> toOption

    let parseInt cul spn = parseInt32 cul spn

    let parseFloat (cul: CultureInfo) (spn: ReadOnlySpan<char>) =
        Double.TryParse(spn, NumberStyles.Currency, CultureInfo.InvariantCulture)
        |> toOption

    let parseDecimal (cul: CultureInfo) (spn: ReadOnlySpan<char>) =
        Decimal.TryParse(spn, NumberStyles.Currency, CultureInfo.InvariantCulture)
        |> toOption

    let parseDateTime (cul: CultureInfo) (spn: ReadOnlySpan<char>) =
        DateTime.TryParse(
            spn,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces
            ||| DateTimeStyles.RoundtripKind
        )
        |> toOption

    let parseDateTimeOffset (cul: CultureInfo) (spn: ReadOnlySpan<char>) =
        DateTime.TryParse(
            spn,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces
            ||| DateTimeStyles.RoundtripKind
        )
        |> toOption

    let parseTimeSpan (cul: CultureInfo) (spn: ReadOnlySpan<char>) =
        TimeSpan.TryParse(spn, CultureInfo.InvariantCulture)
        |> toOption

    let parseBoolean (spn: ReadOnlySpan<char>) = Boolean.TryParse(spn) |> toOption
    let parseGuid (spn: ReadOnlySpan<char>) = Guid.TryParse(spn) |> toOption

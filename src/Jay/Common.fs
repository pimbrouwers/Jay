[<AutoOpen>]
module Common

open System
open System.Globalization

let trySome fn = try fn |> Some with _ -> None

module StringParser =     
    let parseWith (tryParseFunc: string -> bool * _) = 
        tryParseFunc >> function
        | true, v    -> Some v
        | false, _   -> None
      
    let parseInt            cul = parseWith (fun str -> Int32.TryParse(str, NumberStyles.Currency, cul))
    let parseInt16          cul = parseWith (fun str -> Int16.TryParse(str, NumberStyles.Currency, cul))
    let parseInt64          cul = parseWith (fun str -> Int64.TryParse(str, NumberStyles.Currency, cul))
    let parseInt32          cul = parseInt cul
    let parseFloat          cul = parseWith (fun str -> Double.TryParse(str, NumberStyles.Currency, cul))
    let parseDecimal        cul = parseWith (fun str -> Decimal.TryParse(str, NumberStyles.Currency, cul))
    let parseDateTime       cul = parseWith (fun str -> DateTime.TryParse(str, cul, DateTimeStyles.AllowWhiteSpaces ||| DateTimeStyles.RoundtripKind))
    let parseDateTimeOffset cul = parseWith (fun str -> DateTimeOffset.TryParse(str, cul, DateTimeStyles.AllowWhiteSpaces ||| DateTimeStyles.RoundtripKind))
    let parseTimeSpan       cul = parseWith (fun str-> TimeSpan.TryParse(str, cul))
    let parseBoolean            = parseWith Boolean.TryParse
    let parseGuid               = parseWith Guid.TryParse

module UnicodeHelper =
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
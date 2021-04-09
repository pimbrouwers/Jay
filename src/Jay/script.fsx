#r "obj/Debug/net5.0/Jay.dll"

open Jay
open Jay.Json

let json = """
    {
     "level1":
        { "level2":
            { "MyField": "My nested field"}
        }
    }
    """

type MyRecord = { MyNestedField: string option }

module Tweet =
    let fromJson (json: Json) =
        { MyNestedField =
              json
              |> Json.get "level1"
              |> Json.tryGet "level2"
              |> Json.Optional.tryGet "MyField"
              |> JsonExtensions.AsStringOrNone }


json
|> Json.parse
|> Tweet.fromJson
|> printfn "%A"

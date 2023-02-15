module Jay.Tests.Serializer

open System
open FsUnit.Xunit
open Xunit
open Jay

// Serialization
[<Fact>]
let ``Can serialize document with nothing`` () =
    let j = JObject [||] |> Json.serialize
    j |> should equal "{}"

[<Fact>]
let ``Can serialize document with string`` () =
    let txt = "{\"firstName\":\"John\"}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with int`` () =
    let txt = "{\"firstName\":\"John\",\"age\":25}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with bool`` () =
    let txt =
        "{\"firstName\":\"John\",\"employed\":false}"

    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with booleans`` () =
    JObject [| "aa", JBool true
               "bb", JBool false |]
    |> Json.serialize
    |> should equal "{\"aa\":true,\"bb\":false}"

[<Fact>]
let ``Can serialize document with float`` () =
    let txt = "{\"firstName\":\"John\",\"age\":25.25}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with iso 8601 date`` () =
    let txt =
        "{\"birthDate\":\"2020-05-19T14:39:22.500Z\"}"

    let j = Json.parse txt
    j |> Json.serialize |> should equal txt


[<Fact>]
let ``Can serialize document with timespan`` () =
    let txt = "{\"lapTime\":\"00:30:00\"}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with guid`` () =
    let txt =
        "{\"id\":\"{F842213A-82FB-4EEB-AB75-7CCD18676FD5}\"}"

    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with null roundtrip`` () =
    let txt = "{\"firstName\":\"John\",\"age\":null}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with Jnull record`` () =
    let expectedResult = "{\"id\":25,\"text\":null}"

    let actualResult =
        JObject [| "id", JNumber(float 25)
                   "text", JNull |]

        |> Json.serialize

    should equal expectedResult actualResult

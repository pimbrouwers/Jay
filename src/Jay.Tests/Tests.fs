module Tests

open System
open FsUnit.Xunit
open Xunit
open Jay

[<Fact>]
let ``Can parse empty document``() =
    let j = Json.parse "{}"
    j |> should equal (Json.Object [| |])

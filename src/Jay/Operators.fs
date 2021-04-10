[<AutoOpen>]
module Jay.Operators

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

[<RequireQualifiedAccess>]
module Json =
    //Pipeline functions for convenience
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

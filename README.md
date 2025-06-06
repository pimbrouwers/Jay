# Jay

[![NuGet Version](https://img.shields.io/nuget/v/Jay.svg)](https://www.nuget.org/packages/Jay)
[![build](https://github.com/pimbrouwers/Jay/actions/workflows/build.yml/badge.svg)](https://github.com/pimbrouwers/Jay/actions/workflows/build.yml)

The aim of this library was to take the core JSON parser from the amazing [FSharp.Data](https://github.com/fsharp/FSharp.Data/) project, and modernize/simplify it's API.

## Key Features

- Parse JSON from strings, streams, and files.
- Simple and powerful JSON syntax tree.
- Easy conversion between JSON and F# record types.
- Support for primitive type conversions (e.g., `AsString`, `AsInt64`, `AsDateTimeOffset`).

## Design Goals

- Provide a simple means for working with complex JSON structures in F#.
- Define an abstract syntax tree (AST) for JSON that is easy to work with.
- Support for parsing JSON from strings, streams, and files.

## Getting Started

Install the [Jay](https://www.nuget.org/packages/Jay/) NuGet package:

```
PM> Install-Package Jay
```

Or using the dotnet CLI
```cmd
dotnet add package Jay
```

## An Example

Let's consider a stripped down [Tweet object](https://developer.twitter.com/en/docs/tweets/data-dictionary/overview/tweet-object).

> Note: the `user` and `entities` properties have been removed for clarity.

```json
{
 "created_at": "Wed Oct 10 20:19:24 +0000 2018",
 "id": 1050118621198921728,
 "id_str": "1050118621198921728",
 "text": "To make room for more expression, we will now count all emojis as equal—including those with gender‍‍‍ ‍‍and skin t… https://t.co/MkGjXf9aXm",
}
```

In order to work with this in our F# program, we'll first need to create a [record type](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/records).

```f#
type Tweet =
    {
        CreatedAt : DateTimeOffset
        Id        : int64
        IdStr     : string
        Text      : string
    }
```

Next we'll define a module with the same name, `Tweet`, and a function called `fromJson` to consume our JSON and return a `Tweet` record.

```f#
type Tweet =
    {
        CreatedAt : DateTimeOffset
        Id        : int64
        IdStr     : string
        Text      : string
    }

module Tweet =
    let fromJson (json : Json) =
        {
            CreatedAt = json?created_at.AsDateTimeOffset()
            Id        = json?id.AsInt64()
            IdStr     = json?idStr.AsString()
            Text      = json?text.AsString()
        }


let tweetJson = ... // JSON from above
let tweet =
    tweetJson
    |> Json.parse
    |> Tweet.fromJson
```

Finally, we'll create another function `toJson` to convert our record back into JSON represented as an [abstract syntax tree](https://en.wikipedia.org/wiki/Abstract_syntax_tree).

```f#
type Tweet =
    {
        CreatedAt : DateTimeOffset
        Id        : int64
        IdStr     : string
        Text      : string
    }

module Tweet =
    let fromJson (json : Json) =
        {
            CreatedAt = json?created_at.AsDateTimeOffset()
            Id        = json?id.AsInt64()
            IdStr     = json?idStr.AsString()
            Text      = json?text.AsString()
        }

    let toJson (tweet : Tweet) =
        JObject
            [|
                "created_at", JString (tweet.CreatedAt.ToString())
                "id",         JNumber (float tweet.Id)
                "id_str",     JString tweet.IdStr
                "text",       JString tweet.Text
            |]

let tweetJson = ... // JSON from above
let tweet =
    tweetJson
    |> Json.parse
    |> Tweet.fromJson

let json =
    tweet
    |> Tweet.toJson
    |> Json.serialize
```

And that's it!

## Find a bug?

There's an [issue](https://github.com/pimbrouwers/Jay/issues) for that.

## License

Licensed under [MIT](https://github.com/pimbrouwers/Jay/blob/master/LICENSE).

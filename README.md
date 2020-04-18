# Jay

[![NuGet Version](https://img.shields.io/nuget/v/Jay.svg)](https://www.nuget.org/packages/Jay)
[![Build Status](https://travis-ci.org/pimbrouwers/Jay.svg?branch=master)](https://travis-ci.org/pimbrouwers/Jay)

## Getting Started

JSON in F# can be tough and confusing. This library aims to make it much simpler.

Install the [Jay](https://www.nuget.org/packages/Jay/) NuGet package:

```
PM>  Install-Package Jay
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

Next we'll define a static [type extension](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/type-extensions) called `FromJson` to consume our JSON and return a `Tweet` record.

```f#
type Tweet = 
    {
        CreatedAt : DateTimeOffset
        Id        : int64
        IdStr     : string
        Text      : string
    }

    static member FromJson (json : Json) =
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
    |> Tweet.FromJson
```

Finally, we'll create another static type extension `ToJson` to convert our record back into JSON represented as an [abstract syntax tree](https://en.wikipedia.org/wiki/Abstract_syntax_tree).

```f#
type Tweet = 
    {
        CreatedAt : DateTimeOffset
        Id        : int64
        IdStr     : string
        Text      : string
    }

    static member FromJson (json : Json) =
        {
            CreatedAt = json?created_at.AsDateTimeOffset()
            Id        = json?id.AsInt64()
            IdStr     = json?idStr.AsString()
            Text      = json?text.AsString()
        }

    static member ToJson (tweet : Tweet) =
        Json.Object 
            [|
                "created_at", Json.String (tweet.CreatedAt.ToString())
                "id",         Json.Number (float tweet.Id)
                "id_str",     Json.String tweet.IdStr
                "text",       Json.String tweet.Text
            |]

let tweetJson = ... // JSON from above
let tweet = 
    tweetJson
    |> Json.parse
    |> Tweet.FromJson

let json =
    tweet
    |> Tweet.ToJson
    |> Json.serialize
```

And that's it! Not so painful was it?


## A little background

If you're a newcomer to F#, it can be confusing how to handle JSON since there is no commonly accepted approach. Especially if you're coming from C# where you'd normally rely on a reflection-based deserializer. Not requiring you to do anything other than define a class for the JSON to be deserialized into.

Of course F# has JSON type providers, which effectively amount to the same result. Except they generate the types  for you based on sampling the actual JSON you're intending to consume. If you're doing quick-and-dirty exploratory work, type providers are immensely useful. 

F# being so terse, it actually turns out that it is incredible practical to *map your own* JSON (as well as any other IO entry points - see [Donald](https://github.com/pimbrouwers/Donald)). At first this sounds crazy. But if you consider that these IO boundaries are common places for faults. It behoves you to be explicit in those places.

The aim of this library was to take the core JSON parser from the amazing [FSharp.Data](https://github.com/fsharp/FSharp.Data/) project, and modernize/simplify it's API. The hopes of this effort is to make JSON more approachable and easier to reason about for newcomers to the language.

## Find a bug?

There's an [issue](https://github.com/pimbrouwers/Jay/issues) for that.

## License

Built with ♥ by [Pim Brouwers](https://github.com/pimbrouwers) in Toronto, ON. Licensed under [Apache License 2.0](https://github.com/pimbrouwers/Jay/blob/master/LICENSE).

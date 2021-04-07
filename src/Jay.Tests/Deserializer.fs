module Jay.Tests.Deserializer

open System
open FsUnit.Xunit
open Xunit
open Jay

// Parsing
[<Fact>]
let ``Can parse empty document`` () =
    let j = Json.parse "{}"
    j |> should equal (JObject [||])

[<Fact>]
let ``Can parse document with single property`` () =
    let j = Json.parse "{\"firstName\": \"John\"}"

    (j </> "firstName").AsString()
    |> should equal "John"

[<Fact>]
let ``Can parse document with nested property`` () =
    let j =
        Json.parse "{\"name\": {\"first\":\"John\"}}"
        |> Some

    (j <??> "name" <??> "first").AsStringOrNone()
    |> should equal (Some "John")

[<Fact>]
let ``Can parse document with nested optional property`` () =
    let j =
        Json.parse "{\"person\":{\"name\": {\"first\":\"John\"}}}"

    (j </?> "person" <??> "name" <??> "first")
        .AsStringOrNone()
    |> should equal (Some "John")

[<Fact>]
let ``Can parse document with misspelt nested optional property`` () =
    let j =
        Json.parse "{\"person\":{\"name1\": {\"first\":\"John\"}}}"

    (j </?> "person" <??> "name" <??> "first")
        .AsStringOrNone()
    |> should equal (None)

[<Fact>]
let ``Can parse document with missing nested optional property`` () =
    let j =
        Json.parse "{\"person\":{\"name\": {\"first\":\"John\"}}}"

    (j </?> "person" <??> "name" <??> "last")
        .AsStringOrNone()
    |> should equal (None)

[<Fact>]
let ``Can parse document with int16`` () =
    let j =
        Json.parse "{\"firstName\": \"John\", \"age\": 25}"

    j?firstName.AsString() |> should equal "John"
    j?age.AsInt16() |> should equal (25 |> int16)

[<Fact>]
let ``Can parse document with int`` () =
    let j =
        Json.parse "{\"firstName\": \"John\", \"age\": 25}"

    j?age.AsInt() |> should equal 25

[<Fact>]
let ``Can parse document with int64`` () =
    let j =
        Json.parse "{\"firstName\": \"John\", \"age\": 25}"

    j?firstName.AsString() |> should equal "John"
    j?age.AsInt64() |> should equal (25 |> int64)

[<Fact>]
let ``Can parse document with float`` () =
    let j =
        Json.parse "{\"firstName\": \"John\", \"age\": 25.25}"

    j?age.AsDecimal() |> should equal 25.25m

[<Fact>]
let ``Can parse document with decimal`` () =
    let j =
        Json.parse "{\"firstName\": \"John\", \"age\": 25.25}"

    j?age.AsDecimal() |> should equal 25.25M

[<Fact>]
let ``Can parse document with iso 8601 date`` () =
    let j =
        Json.parse "{\"birthDate\": \"2020-05-19T14:39:22.500Z\"}"

    j?birthDate.AsDateTime()
    |> should equal (new DateTime(2020, 5, 19, 14, 39, 22, 500))

[<Fact>]
let ``Can parse document with unix epoch timestamp`` () =
    let j =
        Json.parse "{\"birthDate\": 1587147118004}"

    j?birthDate.AsDateTime()
    |> should equal (new DateTime(2020, 4, 17, 18, 11, 58, 4))

[<Fact>]
let ``Can parse document with datetimeoffset`` () =
    let dtOffset =
        new DateTimeOffset(2020, 4, 17, 18, 11, 58, TimeSpan.FromHours(float -4))

    let j =
        Json.parse (sprintf "{\"birthDate\": \"%O\"}" dtOffset)

    j?birthDate.AsDateTimeOffset()
    |> should equal dtOffset

[<Fact>]
let ``Can parse document with timespan`` () =
    let j = Json.parse "{\"lapTime\": \"00:30:00\"}"

    j?lapTime.AsTimeSpan()
    |> should equal (new TimeSpan(0, 30, 0))

[<Fact>]
let ``Can parse document with guid`` () =
    let j =
        Json.parse "{ \"id\": \"{F842213A-82FB-4EEB-AB75-7CCD18676FD5}\" }"

    j?id.AsGuid()
    |> should equal (Guid.Parse "F842213A-82FB-4EEB-AB75-7CCD18676FD5")

[<Fact>]
let ``Can parse a string from twitter api without throwing an error`` () =
    let txt =
        "[{\"in_reply_to_status_id_str\":\"115445959386861568\",\"truncated\":false,\"in_reply_to_user_id_str\":\"40453522\",\"geo\":null,\"retweet_count\":0,\"contributors\":null,\"coordinates\":null,\"user\":{\"default_profile\":false,\"statuses_count\":3638,\"favourites_count\":28,\"protected\":false,\"profile_text_color\":\"634047\",\"profile_image_url\":\"http:\\/\\/a3.twimg.com\\/profile_images\\/1280550984\\/buddy_lueneburg_normal.jpg\",\"name\":\"Steffen Forkmann\",\"profile_sidebar_fill_color\":\"E3E2DE\",\"listed_count\":46,\"following\":true,\"profile_background_tile\":false,\"utc_offset\":3600,\"description\":\"C#, F# and Dynamics NAV developer, blogger and sometimes speaker. Creator of FAKE - F# Make and NaturalSpec.\",\"location\":\"Hamburg \\/ Germany\",\"contributors_enabled\":false,\"verified\":false,\"profile_link_color\":\"088253\",\"followers_count\":471,\"url\":\"http:\\/\\/www.navision-blog.de\\/blog-mitglieder\\/steffen-forkmann-ueber-mich\\/\",\"profile_sidebar_border_color\":\"D3D2CF\",\"screen_name\":\"sforkmann\",\"default_profile_image\":false,\"notifications\":false,\"show_all_inline_media\":false,\"geo_enabled\":true,\"profile_use_background_image\":true,\"friends_count\":373,\"id_str\":\"22477880\",\"is_translator\":false,\"lang\":\"en\",\"time_zone\":\"Berlin\",\"created_at\":\"Mon Mar 02 12:04:39 +0000 2009\",\"profile_background_color\":\"EDECE9\",\"id\":22477880,\"follow_request_sent\":false,\"profile_background_image_url_https\":\"https:\\/\\/si0.twimg.com\\/images\\/themes\\/theme3\\/bg.gif\",\"profile_background_image_url\":\"http:\\/\\/a1.twimg.com\\/images\\/themes\\/theme3\\/bg.gif\",\"profile_image_url_https\":\"https:\\/\\/si0.twimg.com\\/profile_images\\/1280550984\\/buddy_lueneburg_normal.jpg\"},\"favorited\":false,\"in_reply_to_screen_name\":\"ovatsus\",\"source\":\"\\u003Ca href=\\\"http:\\/\\/www.tweetdeck.com\\\" rel=\\\"nofollow\\\"\\u003ETweetDeck\\u003C\\/a\\u003E\",\"id_str\":\"115447331628916736\",\"in_reply_to_status_id\":115445959386861568,\"id\":115447331628916736,\"created_at\":\"Sun Sep 18 15:29:23 +0000 2011\",\"place\":null,\"retweeted\":false,\"in_reply_to_user_id\":40453522,\"text\":\"@ovatsus I know it's not complete. But I don't want to add a dependency on FParsec in #FSharp.Data. Can you send me samples where it fails?\"},{\"in_reply_to_status_id_str\":null,\"truncated\":false,\"in_reply_to_user_id_str\":null,\"geo\":null,\"retweet_count\":0,\"contributors\":null,\"coordinates\":null,\"user\":{\"statuses_count\":3637,\"favourites_count\":28,\"protected\":false,\"profile_text_color\":\"634047\",\"profile_image_url\":\"http:\\/\\/a3.twimg.com\\/profile_images\\/1280550984\\/buddy_lueneburg_normal.jpg\",\"name\":\"Steffen Forkmann\",\"profile_sidebar_fill_color\":\"E3E2DE\",\"listed_count\":46,\"following\":true,\"profile_background_tile\":false,\"utc_offset\":3600,\"description\":\"C#, F# and Dynamics NAV developer, blogger and sometimes speaker. Creator of FAKE - F# Make and NaturalSpec.\",\"location\":\"Hamburg \\/ Germany\",\"contributors_enabled\":false,\"verified\":false,\"profile_link_color\":\"088253\",\"followers_count\":471,\"url\":\"http:\\/\\/www.navision-blog.de\\/blog-mitglieder\\/steffen-forkmann-ueber-mich\\/\",\"profile_sidebar_border_color\":\"D3D2CF\",\"screen_name\":\"sforkmann\",\"default_profile_image\":false,\"notifications\":false,\"show_all_inline_media\":false,\"geo_enabled\":true,\"profile_use_background_image\":true,\"friends_count\":372,\"id_str\":\"22477880\",\"is_translator\":false,\"lang\":\"en\",\"time_zone\":\"Berlin\",\"created_at\":\"Mon Mar 02 12:04:39 +0000 2009\",\"profile_background_color\":\"EDECE9\",\"id\":22477880,\"default_profile\":false,\"follow_request_sent\":false,\"profile_background_image_url_https\":\"https:\\/\\/si0.twimg.com\\/images\\/themes\\/theme3\\/bg.gif\",\"profile_background_image_url\":\"http:\\/\\/a1.twimg.com\\/images\\/themes\\/theme3\\/bg.gif\",\"profile_image_url_https\":\"https:\\/\\/si0.twimg.com\\/profile_images\\/1280550984\\/buddy_lueneburg_normal.jpg\"},\"favorited\":false,\"in_reply_to_screen_name\":null,\"source\":\"\\u003Ca href=\\\"http:\\/\\/www.tweetdeck.com\\\" rel=\\\"nofollow\\\"\\u003ETweetDeck\\u003C\\/a\\u003E\",\"id_str\":\"115444490331889664\",\"in_reply_to_status_id\":null,\"id\":115444490331889664,\"created_at\":\"Sun Sep 18 15:18:06 +0000 2011\",\"possibly_sensitive\":false,\"place\":null,\"retweeted\":false,\"in_reply_to_user_id\":null,\"text\":\"Added a simple Json parser to #FSharp.Data http:\\/\\/t.co\\/3JGI56SM - #fsharp\"}]"

    Json.parse txt |> ignore

[<Fact>]
let ``Can convert missing optional string field to None`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\" }"

    (j </?> "name").AsStringOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional string field to Some`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\" }"

    j?firstName.AsStringOrNone()
    |> should equal (Some "Don")

[<Fact>]
let ``Can convert missing optional int16 field to None`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\" }"

    (j </?> "age").AsInt16OrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional int16 field to Some`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\", \"age\": 45 }"

    j?age.AsInt16OrNone()
    |> should equal (Some(int16 45))

[<Fact>]
let ``Can convert missing optional int32 field to None`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\" }"

    (j </?> "age").AsInt32OrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional int32 field to Some`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\", \"age\": 45 }"

    (j </?> "age").AsInt32OrNone()
    |> should equal (Some(int32 45))

[<Fact>]
let ``Can convert missing optional int64 field to None`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\" }"

    (j </?> "age").AsInt64OrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional int64 field to Some`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\", \"age\": 45 }"

    (j </?> "age").AsInt64OrNone()
    |> should equal (Some(int64 45))

[<Fact>]
let ``Can convert missing optional int field to None`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\" }"

    (j </?> "age").AsIntOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional int field to Some`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\", \"age\": 45 }"

    j?age.AsIntOrNone() |> should equal (Some(int 45))

[<Fact>]
let ``Can convert missing optional float field to None`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\" }"

    (j </?> "height").AsFloatOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional float field to Some`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\", \"height\": 5.8 }"

    (j </> "height").AsFloatOrNone()
    |> should equal (Some(float 5.8m))

[<Fact>]
let ``Can convert missing optional decimal field to None`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\" }"

    (j </?> "height").AsDecimalOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional decimal field to Some`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\", \"height\": 5.8 }"

    j?height.AsDecimalOrNone()
    |> should equal (Some(decimal 5.8M))

[<Fact>]
let ``Can convert missing optional boolean field to None`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\" }"

    (j </?> "isMale").AsBoolOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional boolean field to Some`` () =
    let j =
        Json.parse "{ \"firstName\": \"Don\", \"lastName\": \"Syme\", \"isMale\": \"true\" }"

    (j </?> "isMale").AsBoolOrNone()
    |> should equal (Some true)

[<Fact>]
let ``Can convert optional iso 8601 date to None`` () =
    let j =
        Json.parse "{\"birthDate\": \"2020-05-19T14:39:22.500Z\"}"

    (j </?> "birthDate1").AsDateTimeOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional iso 8601 date to Some`` () =
    let j =
        Json.parse "{\"birthDate\": \"2020-05-19T14:39:22.500Z\"}"

    (j </?> "birthDate").AsDateTimeOrNone()
    |> should equal (new DateTime(2020, 5, 19, 14, 39, 22, 500) |> Some)

[<Fact>]
let ``Can convert optional unix epoch timestamp to None`` () =
    let j =
        Json.parse "{\"birthDate\": 1587147118004}"

    (j </?> "birthDate1").AsDateTimeOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional unix epoch timestamp to Some`` () =
    let j =
        Json.parse "{\"birthDate\": 1587147118004}"

    (j </> "birthDate").AsDateTimeOrNone()
    |> should equal (new DateTime(2020, 4, 17, 18, 11, 58, 4) |> Some)

[<Fact>]
let ``Can convert optional datetimeoffset to None`` () =
    let dtOffset =
        new DateTimeOffset(2020, 4, 17, 18, 11, 58, TimeSpan.FromHours(float -4))

    let j =
        Json.parse (sprintf "{\"birthDate\": \"%O\"}" dtOffset)

    (j </?> "birthDate1").AsDateTimeOffsetOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional datetimeoffset to Some`` () =
    let dtOffset =
        new DateTimeOffset(2020, 4, 17, 18, 11, 58, TimeSpan.FromHours(float -4))

    let j =
        Json.parse (sprintf "{\"birthDate\": \"%O\"}" dtOffset)

    (j </?> "birthDate").AsDateTimeOffsetOrNone()
    |> should equal (dtOffset |> Some)

[<Fact>]
let ``Can convert optional timespan to None`` () =
    let j = Json.parse "{\"lapTime\": \"00:30:00\"}"

    (j </?> "lapTime1").AsTimeSpanOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional timespan to Some`` () =
    let j = Json.parse "{\"lapTime\": \"00:30:00\"}"

    (j </?> "lapTime").AsTimeSpanOrNone()
    |> should equal (new TimeSpan(0, 30, 0) |> Some)

[<Fact>]
let ``Can convert optional guid to None`` () =
    let j =
        Json.parse "{ \"id\": \"{F842213A-82FB-4EEB-AB75-7CCD18676FD5}\" }"

    (j </?> "id1").AsGuidOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional guid to Some`` () =
    let j =
        Json.parse "{ \"id\": \"{F842213A-82FB-4EEB-AB75-7CCD18676FD5}\" }"

    (j </?> "id").AsGuidOrNone()
    |> should
        equal
        (Guid.Parse "F842213A-82FB-4EEB-AB75-7CCD18676FD5"
         |> Some)

[<Fact>]
let ``Can parse array of numbers`` () =
    let j = Json.parse "[1, 2, 3]"
    j.[0] |> should equal (JNumber 1.0)
    j.[1] |> should equal (JNumber 2.0)
    j.[2] |> should equal (JNumber 3.0)

[<Fact>]
let ``Quotes in strings are properly escaped`` () =
    let txt =
        "{\"short_description\":\"This a string with \\\"quotes\\\"\"}"

    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can convert optional array to None`` () =
    let j = Json.parse "{ \"nos\": [1, 2, 3] }"

    (j </?> "nos1").AsArrayOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional array to Some`` () =
    let j = Json.parse "{ \"nos\": [1, 2, 3] }"

    Option.get((j </?> "nos").AsArrayOrNone()).Length
    |> should equal (3)

[<Fact>]
let ``Can convert optional obj to None`` () =
    let j =
        Json.parse "{ \"name\": { \"firstName\": \"Don Syme\" } }"

    (j </?> "name1").AsArrayOrNone()
    |> should equal (None)

[<Fact>]
let ``Can convert optional obj to Some`` () =
    let j =
        Json.parse "{ \"name\": { \"firstName\": \"Don Syme\" } }"

    Option
        .get(
            (j </?> "name").AsPropertyArrayOrNone()
        )
        .Length
    |> should equal (1)

[<Fact>]
let ``Can deserialize document with null to object`` () =
    let txt = "{\"firstName\":\"John\"}"
    let j = Json.parse txt
    let expectedResult = {| firstName = "John"; age = None |}

    let parsedResult =
        {| firstName = j?firstName.AsString()
           age = (j </?> "age").AsInt16OrNone() |}

    should equal (expectedResult = parsedResult) true

[<Fact>]
let ``Can deserialize document with explicit nullable string to None`` () =
    let txt = "{\"firstName\":null}"
    let j = Json.parse txt |> Some
    let expectedResult = {| firstName = None |}

    let parsedResult =
        {| firstName = (j <??> "firstName").AsStringOrNone() |}

    should equal (parsedResult = expectedResult) true

[<Fact>]
let ``Can deserialize document with explicit nullable int to None`` () =
    let txt = "{\"age\":null}"
    let j = Json.parse txt
    let expectedResult = {| age = None |}
    let parsedResult = {| age = j?age.AsInt32OrNone() |}
    should equal (parsedResult = expectedResult) true

[<Fact>]
let ``Can deserialize document with explicit nullable int64 to None`` () =
    let txt = "{\"age\":null}"
    let j = Json.parse txt
    let expectedResult = {| age = None |}
    let parsedResult = {| age = j?age.AsInt64OrNone() |}
    should equal (parsedResult = expectedResult) true

[<Fact>]
let ``Can deserialize document with explicit nullable int16 to None`` () =
    let txt = "{\"age\":null}"
    let j = Json.parse txt
    let expectedResult = {| age = None |}
    let parsedResult = {| age = j?age.AsInt16OrNone() |}
    should equal (parsedResult = expectedResult) true

[<Fact>]
let ``Can deserialize document with explicit nullable float to None`` () =
    let txt = "{\"age\":null}"
    let j = Json.parse txt
    let expectedResult = {| age = None |}
    let parsedResult = {| age = j?age.AsFloatOrNone() |}
    should equal (parsedResult = expectedResult) true

[<Fact>]
let ``Can deserialize document with explicit nullable decimal to None`` () =
    let txt = "{\"age\":null}"
    let j = Json.parse txt
    let expectedResult = {| age = None |}
    let parsedResult = {| age = j?age.AsDecimalOrNone() |}
    should equal (parsedResult = expectedResult) true

[<Fact>]
let ``Can deserialize document with explicit nullable array to None`` () =
    let txt = "{\"age\":null}"
    let j = Json.parse txt
    let expectedResult = {| age = None |}
    let parsedResult = {| age = j?age.AsArrayOrNone() |}
    should equal (parsedResult = expectedResult) true

[<Fact>]
let ``Can deserialize document with explicit nullable object to None`` () =
    let txt = "{\"age\":null}"
    let j = Json.parse txt
    let expectedResult = {| age = None |}

    let parsedResult =
        {| age = j?age.AsPropertyArrayOrNone() |}

    should equal (parsedResult = expectedResult) true

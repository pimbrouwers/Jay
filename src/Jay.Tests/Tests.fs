module Jay.Tests

open System
open FsUnit.Xunit
open Xunit
open Jay

// Parsing
[<Fact>]
let ``Can parse empty document``() =
    let j = Json.parse "{}"
    j |> should equal (JObject [| |])

[<Fact>]
let ``Can parse document with single property``() =
    let j = Json.parse "{\"firstName\": \"John\"}"
    j?firstName.AsString() |> should equal "John"

[<Fact>]
let ``Can parse document with int16``() =
    let j = Json.parse "{\"firstName\": \"John\", \"age\": 25}"
    j?firstName.AsString() |> should equal "John"
    j?age.AsInt16()  |> should equal (25 |> int16)

[<Fact>]
let ``Can parse document with int``() =
    let j = Json.parse "{\"firstName\": \"John\", \"age\": 25}"
    j?firstName.AsString() |> should equal "John"
    j?age.AsInt()  |> should equal 25

[<Fact>]
let ``Can parse document with int64``() =
    let j = Json.parse "{\"firstName\": \"John\", \"age\": 25}"
    j?firstName.AsString() |> should equal "John"
    j?age.AsInt64()  |> should equal (25 |> int64)
    
[<Fact>]
let ``Can parse document with float``() =
    let j = Json.parse "{\"firstName\": \"John\", \"age\": 25.25}"
    j?age.AsDecimal() |> should equal 25.25m

[<Fact>]
let ``Can parse document with decimal``() =
    let j = Json.parse "{\"firstName\": \"John\", \"age\": 25.25}"
    j?age.AsDecimal() |> should equal 25.25M

[<Fact>]
let ``Can parse document with iso 8601 date``() =
    let j = Json.parse "{\"birthDate\": \"2020-05-19T14:39:22.500Z\"}"
    j?birthDate.AsDateTime() |> should equal (new DateTime(2020, 05, 19, 14, 39, 22, 500))    

[<Fact>]
let ``Can parse document with unix epoch timestamp``() =
    let j = Json.parse "{\"birthDate\": 1587147118004}"    
    j?birthDate.AsDateTime() |> should equal (new DateTime(2020, 04, 17, 18, 11, 58, 4))    

[<Fact>]
let ``Can parse document with datetimeoffset`` () =
    let dtOffset = new DateTimeOffset(2020, 04, 17, 18, 11, 58, TimeSpan.FromHours(float -4));    
    let j = Json.parse (sprintf "{\"birthDate\": \"%O\"}" dtOffset)    
    j?birthDate.AsDateTimeOffset() |> should equal dtOffset

[<Fact>]
let ``Can parse document with timespan``() =
    let j = Json.parse "{\"lapTime\": \"00:30:00\"}"    
    j?lapTime.AsTimeSpan() |> should equal (new TimeSpan(0, 30,0))    

[<Fact>]
let ``Can parse document with guid``() =
    let j = Json.parse "{ \"id\": \"{F842213A-82FB-4EEB-AB75-7CCD18676FD5}\" }"
    j?id.AsGuid() |> should equal (Guid.Parse "F842213A-82FB-4EEB-AB75-7CCD18676FD5")
    
[<Fact>]
let ``Can parse a string from twitter api without throwing an error``() =
    let txt =
        "[{\"in_reply_to_status_id_str\":\"115445959386861568\",\"truncated\":false,\"in_reply_to_user_id_str\":\"40453522\",\"geo\":null,\"retweet_count\":0,\"contributors\":null,\"coordinates\":null,\"user\":{\"default_profile\":false,\"statuses_count\":3638,\"favourites_count\":28,\"protected\":false,\"profile_text_color\":\"634047\",\"profile_image_url\":\"http:\\/\\/a3.twimg.com\\/profile_images\\/1280550984\\/buddy_lueneburg_normal.jpg\",\"name\":\"Steffen Forkmann\",\"profile_sidebar_fill_color\":\"E3E2DE\",\"listed_count\":46,\"following\":true,\"profile_background_tile\":false,\"utc_offset\":3600,\"description\":\"C#, F# and Dynamics NAV developer, blogger and sometimes speaker. Creator of FAKE - F# Make and NaturalSpec.\",\"location\":\"Hamburg \\/ Germany\",\"contributors_enabled\":false,\"verified\":false,\"profile_link_color\":\"088253\",\"followers_count\":471,\"url\":\"http:\\/\\/www.navision-blog.de\\/blog-mitglieder\\/steffen-forkmann-ueber-mich\\/\",\"profile_sidebar_border_color\":\"D3D2CF\",\"screen_name\":\"sforkmann\",\"default_profile_image\":false,\"notifications\":false,\"show_all_inline_media\":false,\"geo_enabled\":true,\"profile_use_background_image\":true,\"friends_count\":373,\"id_str\":\"22477880\",\"is_translator\":false,\"lang\":\"en\",\"time_zone\":\"Berlin\",\"created_at\":\"Mon Mar 02 12:04:39 +0000 2009\",\"profile_background_color\":\"EDECE9\",\"id\":22477880,\"follow_request_sent\":false,\"profile_background_image_url_https\":\"https:\\/\\/si0.twimg.com\\/images\\/themes\\/theme3\\/bg.gif\",\"profile_background_image_url\":\"http:\\/\\/a1.twimg.com\\/images\\/themes\\/theme3\\/bg.gif\",\"profile_image_url_https\":\"https:\\/\\/si0.twimg.com\\/profile_images\\/1280550984\\/buddy_lueneburg_normal.jpg\"},\"favorited\":false,\"in_reply_to_screen_name\":\"ovatsus\",\"source\":\"\\u003Ca href=\\\"http:\\/\\/www.tweetdeck.com\\\" rel=\\\"nofollow\\\"\\u003ETweetDeck\\u003C\\/a\\u003E\",\"id_str\":\"115447331628916736\",\"in_reply_to_status_id\":115445959386861568,\"id\":115447331628916736,\"created_at\":\"Sun Sep 18 15:29:23 +0000 2011\",\"place\":null,\"retweeted\":false,\"in_reply_to_user_id\":40453522,\"text\":\"@ovatsus I know it's not complete. But I don't want to add a dependency on FParsec in #FSharp.Data. Can you send me samples where it fails?\"},{\"in_reply_to_status_id_str\":null,\"truncated\":false,\"in_reply_to_user_id_str\":null,\"geo\":null,\"retweet_count\":0,\"contributors\":null,\"coordinates\":null,\"user\":{\"statuses_count\":3637,\"favourites_count\":28,\"protected\":false,\"profile_text_color\":\"634047\",\"profile_image_url\":\"http:\\/\\/a3.twimg.com\\/profile_images\\/1280550984\\/buddy_lueneburg_normal.jpg\",\"name\":\"Steffen Forkmann\",\"profile_sidebar_fill_color\":\"E3E2DE\",\"listed_count\":46,\"following\":true,\"profile_background_tile\":false,\"utc_offset\":3600,\"description\":\"C#, F# and Dynamics NAV developer, blogger and sometimes speaker. Creator of FAKE - F# Make and NaturalSpec.\",\"location\":\"Hamburg \\/ Germany\",\"contributors_enabled\":false,\"verified\":false,\"profile_link_color\":\"088253\",\"followers_count\":471,\"url\":\"http:\\/\\/www.navision-blog.de\\/blog-mitglieder\\/steffen-forkmann-ueber-mich\\/\",\"profile_sidebar_border_color\":\"D3D2CF\",\"screen_name\":\"sforkmann\",\"default_profile_image\":false,\"notifications\":false,\"show_all_inline_media\":false,\"geo_enabled\":true,\"profile_use_background_image\":true,\"friends_count\":372,\"id_str\":\"22477880\",\"is_translator\":false,\"lang\":\"en\",\"time_zone\":\"Berlin\",\"created_at\":\"Mon Mar 02 12:04:39 +0000 2009\",\"profile_background_color\":\"EDECE9\",\"id\":22477880,\"default_profile\":false,\"follow_request_sent\":false,\"profile_background_image_url_https\":\"https:\\/\\/si0.twimg.com\\/images\\/themes\\/theme3\\/bg.gif\",\"profile_background_image_url\":\"http:\\/\\/a1.twimg.com\\/images\\/themes\\/theme3\\/bg.gif\",\"profile_image_url_https\":\"https:\\/\\/si0.twimg.com\\/profile_images\\/1280550984\\/buddy_lueneburg_normal.jpg\"},\"favorited\":false,\"in_reply_to_screen_name\":null,\"source\":\"\\u003Ca href=\\\"http:\\/\\/www.tweetdeck.com\\\" rel=\\\"nofollow\\\"\\u003ETweetDeck\\u003C\\/a\\u003E\",\"id_str\":\"115444490331889664\",\"in_reply_to_status_id\":null,\"id\":115444490331889664,\"created_at\":\"Sun Sep 18 15:18:06 +0000 2011\",\"possibly_sensitive\":false,\"place\":null,\"retweeted\":false,\"in_reply_to_user_id\":null,\"text\":\"Added a simple Json parser to #FSharp.Data http:\\/\\/t.co\\/3JGI56SM - #fsharp\"}]"
    Json.parse txt |> ignore

[<Fact>]
let ``Can parse array of numbers``() =
    let j = Json.parse "[1, 2, 3]"
    j.[0] |> should equal (JNumber 1.0)
    j.[1] |> should equal (JNumber 2.0)
    j.[2] |> should equal (JNumber 3.0)

[<Fact>]
let ``Quotes in strings are properly escaped``() =
    let txt = "{\"short_description\":\"This a string with \\\"quotes\\\"\"}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

// Serialization
[<Fact>]
let ``Can serialize document with nothing``() =
    let j = JObject [||] |> Json.serialize
    j |> should equal "{}"

[<Fact>]
let ``Can serialize document with string``() =
    let txt = "{\"firstName\":\"John\"}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with int``() =
    let txt = "{\"firstName\":\"John\",\"age\":25}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with bool``() =
    let txt = "{\"firstName\":\"John\",\"employed\":false}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with booleans``() =
    JObject 
        [| 
            "aa", JBool true
            "bb", JBool false 
        |]
    |> Json.serialize
    |> should equal "{\"aa\":true,\"bb\":false}"

[<Fact>]
let ``Can serialize document with float``() =
    let txt = "{\"firstName\":\"John\",\"age\":25.25}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with iso 8601 date``() =
    let txt = "{\"birthDate\":\"2020-05-19T14:39:22.500Z\"}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt


[<Fact>]
let ``Can serialize document with timespan``() =
    let txt = "{\"lapTime\":\"00:30:00\"}"    
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt

[<Fact>]
let ``Can serialize document with guid``() =
    let txt = "{\"id\":\"{F842213A-82FB-4EEB-AB75-7CCD18676FD5}\"}"
    let j = Json.parse txt
    j |> Json.serialize |> should equal txt
  
namespace ffrab.mobile.common


module model =

    open Xamarin.Forms
    open System
    open FSharp.Data
    open NodaTime
   
    type Entry = {
        Id : int;
        Guid: Guid;
        Title: string;
        Subtitle: string;
        Track: string;
        Start: OffsetDateTime;
        Duration: Duration;
        Type: string;
        Language: string;
        Abstract: string;
        Description: string;
    }

    type Room = {
        Name : string;
        Entries : Entry list;
    }

    type ConferenceDay = {
        Index : int;
        Day : LocalDate;
        StartTime : OffsetDateTime;
        EndTime : OffsetDateTime;
        Rooms : Room list
    }

    type ConferenceData = {
        Days : ConferenceDay list;
        Version : string
    }

    [<AllowNullLiteral>]
    type Conference(id, name, dataUri) =
        let id = id
        let name = name
        let dataUri = dataUri
        let mutable data : ConferenceData option = None
        
        member this.Name with get() = name
        member this.Id with get() = id
        member this.DataUri with get() = dataUri
        member this.Data with get() = data and set(v) = data <- v
        


    module Conferences =

        open Newtonsoft.Json
        open Newtonsoft.Json.Linq
        open System.IO
        open System.Linq
        open NodaTime.Text
        

        let actualConfKey = "actualConference"

        let conferences = 
            [
                new Conference(1, "Linuxwochen 2013", "https://cfp.linuxwochen.at/en/LWW13/public/schedule.json")
                new Conference(2, "Linuxwochen 2014", "https://cfp.linuxwochen.at/en/LWW14/public/schedule.json")
                new Conference(3, "Linuxwochen 2015", "https://cfp.linuxwochen.at/en/LWW15/public/schedule.json") 
            ]

        let getAllConferences() =
            conferences


        let getConference id =
            conferences |> List.tryFind (fun i -> i.Id = id)

        let setActualConference (conf : Conference) =
            Application.Current.Properties.[actualConfKey] <- conf.Id

        let getActualConference () =
            let keyExists = Application.Current.Properties.ContainsKey(actualConfKey)
            
            match keyExists with
            | true ->
                let id = Application.Current.Properties.[actualConfKey] :?> int
                getConference id
            | _ ->
                None

        let loadJsonFromUri (uri : string)  =
            async {
                let httpClient = new System.Net.Http.HttpClient()
                let! response = httpClient.GetAsync(uri) |> Async.AwaitTask
                response.EnsureSuccessStatusCode () |> ignore
                let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return content
            } |> Async.RunSynchronously

        let dateFormat = LocalDatePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd")
        let dateTimeFormat = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'sso<G>")
        let durationFormat = DurationPattern.CreateWithInvariantCulture("H:ss")
        
        let deserializeJson data =
            let reader = new JsonTextReader(new StringReader(data));
            reader.DateParseHandling <- DateParseHandling.None;
            JObject.Load(reader);

        let parseSchedule (root : JObject) =
            let scheduleNode = root.["schedule"]
            let conferenceNode = scheduleNode.["conference"] :?> JObject

            let version = scheduleNode.["version"]
            
            ({ Version = version.Value<string>(); Days = [] }, conferenceNode)

        let parseEntry (entryNode : JToken) =
            {
                Id = entryNode.["id"].Value<int>();
                Guid = Guid.Parse(entryNode.["guid"].Value<string>());
                Title = entryNode.["title"].Value<string>();
                Subtitle = entryNode.["subtitle"].Value<string>();
                Abstract = entryNode.["abstract"].Value<string>();
                Track = entryNode.["track"].Value<string>();
                Description = entryNode.["description"].Value<string>();
                Start = dateTimeFormat.Parse(entryNode.["date"].Value<string>()).Value;
                Language = entryNode.["language"].Value<string>();
                Duration = durationFormat.Parse(entryNode.["duration"].Value<string>()).Value;
                Type = entryNode.["type"].Value<string>();
            }

        let parseRoom (roomNode : JToken) =
            let roomPropertyNode = roomNode :?> JProperty

            let array = roomPropertyNode.First.Children()

            let entries = array |>
                          List.ofSeq |>
                          List.map parseEntry

            { 
                Name = roomPropertyNode.Name;
                Entries = entries
            }


        let parseDay (dayNode : JToken) =
            let date = dayNode.["date"]

            let day = dateFormat.Parse(date.Value<string>()).Value
            let startTime = dateTimeFormat.Parse(dayNode.["day_start"].Value<string>()).Value
            let endTime = dateTimeFormat.Parse(dayNode.["day_end"].Value<string>()).Value
            
            let rooms = dayNode.["rooms"].Children() |>
                        List.ofSeq |>
                        List.map parseRoom

            {
                Index = dayNode.["index"].Value<int>();
                Day = day;
                StartTime = startTime;
                EndTime = endTime;
                Rooms = rooms
            }

        let parseDays (confData : ConferenceData, conferenceNode : JObject) =
            let daysNode = conferenceNode.["days"]

            let days = daysNode.Children() |>
                       List.ofSeq |>
                       List.map parseDay
            
            { confData with Days = days }

        let parseJson json =
            let root = deserializeJson json

            let conferenceData = root |>
                                 parseSchedule |>
                                 parseDays


            conferenceData
            
        let getConferenceData (conf : Conference)=
            match conf.Data with
            | Some data ->
                data
            | _ ->
                let data = loadJsonFromUri conf.DataUri |>
                           parseJson

                conf.Data <- Some data
                data
                
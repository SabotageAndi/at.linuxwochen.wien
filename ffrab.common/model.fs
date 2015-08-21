namespace ffrab.mobile.common


module model =

    open Xamarin.Forms
    open System
    open FSharp.Data
    open NodaTime
    open SQLite.Net
    open SQLite.Net.Attributes


    type Entry() = 
        member val Id : int = -1 with get,set
        [<PrimaryKeyAttribute()>]
        member val Guid: Guid = Guid.Empty with get, set
        member val Title: string = "" with get, set
        member val Subtitle: string = "" with get, set
        [<IndexedAttribute()>]
        member val Track: string = "" with get, set
        member val Start: OffsetDateTime = new OffsetDateTime() with  get, set
        member val Duration: Duration = Duration.Zero with get, set
        member val Type: string = "" with get, set
        member val Language: string = "" with get, set
        member val Abstract: string =  "" with get, set
        member val Description: string = "" with get, set

        [<IndexedAttribute()>]
        member val ConferenceId : int = 0 with get, set
        [<IndexedAttribute()>]
        member val ConferenceDayGuid : Guid = Guid.Empty with get, set
        [<IndexedAttribute()>]
        member val RoomGuid : Guid = Guid.Empty with get, set
    

    type Room() = 
        [<PrimaryKeyAttribute()>]
        member val Guid : Guid = Guid.Empty with get, set

        [<IndexedAttribute()>]
        member val ConferenceId : int = 0 with get, set
        [<IndexedAttribute()>]
        member val ConferenceDayGuid : Guid = Guid.Empty with get, set

        member val Name : string = "" with get, set
        member val Entries : Entry list = [] with get, set
    

    type ConferenceDay() = 
        [<PrimaryKeyAttribute()>]
        member val Guid : Guid = Guid.Empty with get, set

        [<IndexedAttribute()>]
        member val ConferenceId : int = 0 with get, set
        member val Index : int = -1 with get, set
        member val Day : LocalDate = new LocalDate() with get, set
        member val StartTime : OffsetDateTime = new OffsetDateTime() with get, set
        member val EndTime : OffsetDateTime = new OffsetDateTime() with get, set
        member val Rooms : Room list = [] with get, set
    

    type ConferenceData() = 
        [<PrimaryKeyAttribute()>]
        member val ConferenceId : int = 0 with get, set
        member val Days : ConferenceDay list = [] with get, set
        member val Version : string = "" with get, set
    

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

        module Parser = 
            let dateFormat = LocalDatePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd")
            let dateTimeFormat = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'sso<G>")
            let durationFormat = DurationPattern.CreateWithInvariantCulture("H:ss")
        
            let deserializeJson data =
                let reader = new JsonTextReader(new StringReader(data));
                reader.DateParseHandling <- DateParseHandling.None;
                JObject.Load(reader);

          
            let parseEntry (room : Room) (entryNode : JToken) =
                new Entry(
                    Id = entryNode.["id"].Value<int>(),
                    Guid = Guid.Parse(entryNode.["guid"].Value<string>()),
                    Title = entryNode.["title"].Value<string>(),
                    Subtitle = entryNode.["subtitle"].Value<string>(),
                    Abstract = entryNode.["abstract"].Value<string>(),
                    Track = entryNode.["track"].Value<string>(),
                    Description = entryNode.["description"].Value<string>(),
                    Start = dateTimeFormat.Parse(entryNode.["date"].Value<string>()).Value,
                    Language = entryNode.["language"].Value<string>(),
                    Duration = durationFormat.Parse(entryNode.["duration"].Value<string>()).Value,
                    Type = entryNode.["type"].Value<string>(),
                    ConferenceId = room.ConferenceId,
                    ConferenceDayGuid = room.ConferenceDayGuid,
                    RoomGuid = room.Guid
                )

            let parseRoom (conferenceDay : ConferenceDay) (roomNode : JToken) =
                let roomPropertyNode = roomNode :?> JProperty

                let array = roomPropertyNode.First.Children()

                let room = new Room(Name = roomPropertyNode.Name, Guid = Guid.NewGuid(), ConferenceId = conferenceDay.ConferenceId, ConferenceDayGuid = conferenceDay.Guid)

                let entries = array |>
                              List.ofSeq |>
                              List.map (fun x -> parseEntry room x)

                room.Entries <- entries
                room



            let parseDay (conference : Conference) (dayNode : JToken) =
                let date = dayNode.["date"]

                let day = dateFormat.Parse(date.Value<string>()).Value
                let startTime = dateTimeFormat.Parse(dayNode.["day_start"].Value<string>()).Value
                let endTime = dateTimeFormat.Parse(dayNode.["day_end"].Value<string>()).Value
            
                let conferenceDay = new ConferenceDay(
                                        Index = dayNode.["index"].Value<int>(),
                                        Day = day,
                                        StartTime = startTime,
                                        EndTime = endTime,
                                        ConferenceId = conference.Id,
                                        Guid = Guid.NewGuid()
                                    )

                let rooms = dayNode.["rooms"].Children() |>
                            List.ofSeq |>
                            List.map (fun x -> parseRoom conferenceDay x)

                conferenceDay.Rooms <- rooms
                conferenceDay

            let parseDays conference (conferenceNode : JObject) =
                let daysNode = conferenceNode.["days"]

                daysNode.Children() |>
                List.ofSeq |>
                List.map (fun x -> parseDay conference x)


            let parseSchedule (conference : Conference) (root : JObject) =
                let scheduleNode = root.["schedule"]
                let conferenceNode = scheduleNode.["conference"] :?> JObject

                let version = scheduleNode.["version"]
                let days = parseDays conference conferenceNode

                new ConferenceData(Version = version.Value<string>(), Days = days, ConferenceId = conference.Id)


            let parseJson conference json =
                deserializeJson json |>
                parseSchedule conference
                
                
            
        let getConferenceData (conf : Conference)=
            match conf.Data with
            | Some data ->
                data
            | _ ->
                let data = loadJsonFromUri conf.DataUri |>
                           Parser.parseJson conf

                conf.Data <- Some data
                data
                
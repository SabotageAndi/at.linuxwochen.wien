namespace ffrab.mobile.common

module model = 
    open Xamarin.Forms
    open System
    open SQLite.Net
    open FSharp.Data
    open entities
    open common
    open System.Collections.Generic
    
    [<AllowNullLiteral>]
    type Conference(id, name, dataUri) = 
        let id = id
        let name = name
        let dataUri = dataUri
        let mutable data : ConferenceData option = None
        member this.Name = name
        member this.Id = id
        member this.DataUri = dataUri
        
        member this.Data 
            with get () = data
            and set (v) = data <- v

    type State(conn) = 
        let sqlConnection : SQLiteConnection = conn

        member this.SQLConnection 
            with get() = sqlConnection
    

    let mutable CurrentState : State = new State(null)

    

    
    module Conferences = 
        open Newtonsoft.Json
        open Newtonsoft.Json.Linq
        open System.IO
        open System.Linq
        open NodaTime.Text
        
        let actualConfKey = "actualConference"
        
        let conferences = 
            [ new Conference(1, "Linuxwochen 2013", "https://cfp.linuxwochen.at/en/LWW13/public/schedule.json")
              new Conference(2, "Linuxwochen 2014", "https://cfp.linuxwochen.at/en/LWW14/public/schedule.json")
              new Conference(3, "Linuxwochen 2015", "https://cfp.linuxwochen.at/en/LWW15/public/schedule.json") ]
        
        let getAllConferences() = conferences
        let getConference id = conferences |> List.tryFind (fun i -> i.Id = id)
        let setActualConference (conf : Conference) = Application.Current.Properties.[actualConfKey] <- conf.Id
        
        let getActualConference() = 
            let keyExists = Application.Current.Properties.ContainsKey(actualConfKey)
            match keyExists with
            | true -> 
                let id = Application.Current.Properties.[actualConfKey] :?> int
                getConference id
            | _ -> None
   
        
        module Parser = 

            
            let deserializeJson data = 
                let reader = new JsonTextReader(new StringReader(data))
                reader.DateParseHandling <- DateParseHandling.None
                JObject.Load(reader)
            
            let parseEntry (room : Room) (entryNode : JToken) = 
                new Entry(Id = entryNode.["id"].Value<int>(), Guid = entryNode.["guid"].Value<string>(), 
                          Title = entryNode.["title"].Value<string>(), Subtitle = entryNode.["subtitle"].Value<string>(), 
                          Abstract = entryNode.["abstract"].Value<string>(), Track = entryNode.["track"].Value<string>(), 
                          Description = entryNode.["description"].Value<string>(), 
                          Start = common.Formatting.dateTimeFormat.Parse(entryNode.["date"].Value<string>()).Value, 
                          Language = entryNode.["language"].Value<string>(), 
                          Duration = common.Formatting.durationFormat.Parse(entryNode.["duration"].Value<string>()).Value, 
                          Type = entryNode.["type"].Value<string>(), ConferenceId = room.ConferenceId, 
                          ConferenceDayGuid = room.ConferenceDayGuid, RoomGuid = room.Guid)
            
            let parseRoom (conferenceDay : ConferenceDay) (roomNode : JToken) = 
                let roomPropertyNode = roomNode :?> JProperty
                let array = roomPropertyNode.First.Children()
                let room = 
                    new Room(Name = roomPropertyNode.Name, Guid = Guid.NewGuid(), 
                             ConferenceId = conferenceDay.ConferenceId, ConferenceDayGuid = conferenceDay.Guid)
                
                let entries = 
                    array
                    |> List.ofSeq
                    |> List.map (fun x -> parseEntry room x)
                room.Entries <- entries
                room
            
            let parseDay (conference : Conference) (dayNode : JToken) = 
                let date = dayNode.["date"]
                let day = common.Formatting.dateFormat.Parse(date.Value<string>()).Value
                let startTime = common.Formatting.dateTimeFormat.Parse(dayNode.["day_start"].Value<string>()).Value
                let endTime = common.Formatting.dateTimeFormat.Parse(dayNode.["day_end"].Value<string>()).Value
                let conferenceDay = 
                    new ConferenceDay(Index = dayNode.["index"].Value<int>(), Day = day, StartTime = startTime, 
                                      EndTime = endTime, ConferenceId = conference.Id, Guid = Guid.NewGuid())
                
                let rooms = 
                    dayNode.["rooms"].Children()
                    |> List.ofSeq
                    |> List.map (fun x -> parseRoom conferenceDay x)
                conferenceDay.Rooms <- rooms
                conferenceDay
            
            let parseDays conference (conferenceNode : JObject) = 
                let daysNode = conferenceNode.["days"]
                daysNode.Children()
                |> List.ofSeq
                |> List.map (fun x -> parseDay conference x)
            
            let parseSchedule (conference : Conference) (root : JObject) = 
                let scheduleNode = root.["schedule"]
                let conferenceNode = scheduleNode.["conference"] :?> JObject
                let version = scheduleNode.["version"]
                let days = parseDays conference conferenceNode
                new ConferenceData(Version = version.Value<string>(), Days = days, ConferenceId = conference.Id)
            
            let parseJson conference json = deserializeJson json |> parseSchedule conference

        module Database = 
            let shouldCreateDatabase() =
                not(CurrentState.SQLConnection.GetTableInfo("Entry").Any())

            let reCreateDatabase() =
                CurrentState.SQLConnection.DropTable<Entry>() |> ignore
                CurrentState.SQLConnection.DropTable<Room>() |> ignore
                CurrentState.SQLConnection.DropTable<ConferenceDay>() |> ignore
                CurrentState.SQLConnection.DropTable<ConferenceData>() |> ignore

                CurrentState.SQLConnection.CreateTable<Entry>() |> ignore
                CurrentState.SQLConnection.CreateTable<Room>() |> ignore
                CurrentState.SQLConnection.CreateTable<ConferenceDay>() |> ignore
                CurrentState.SQLConnection.CreateTable<ConferenceData>() |> ignore


            let deleteConference (conference : Conference) =
                CurrentState.SQLConnection.Table<Entry>().Delete(fun e -> e.ConferenceId = conference.Id) |> ignore
                CurrentState.SQLConnection.Table<Room>().Delete(fun e -> e.ConferenceId = conference.Id) |> ignore
                CurrentState.SQLConnection.Table<ConferenceDay>().Delete(fun e -> e.ConferenceId = conference.Id) |> ignore
                CurrentState.SQLConnection.Table<ConferenceData>().Delete(fun e -> e.ConferenceId = conference.Id) |> ignore

            let getConferenceData (conference : Conference) =
                CurrentState.SQLConnection.Table<ConferenceData>()
                |> Seq.filter (fun c -> c.ConferenceId = conference.Id)
                |> Seq.tryHead

            let writeDbEntry dbEntry =
                CurrentState.SQLConnection.Insert dbEntry |> ignore

            let getConferenceDays (conference : Conference) =
                CurrentState.SQLConnection.Table<ConferenceDay>()
                |> Seq.filter (fun cd -> cd.ConferenceId = conference.Id)

            
            let getEntriesForDay (day : ConferenceDay) =
                CurrentState.SQLConnection.Table<Entry>()
                |> Seq.filter (fun e -> e.ConferenceDayGuid = day.Guid)
                |> Seq.sortBy (fun e -> e.Start.ToDateTimeOffset())
            

        module Synchronization =

            let writeEntry (entry : Entry) =
                Database.writeDbEntry entry

            let writeRoom (room : Room) =
                Database.writeDbEntry room
                room.Entries 
                |> List.iter writeEntry

            let writeDay (conferenceDay : ConferenceDay) =
                Database.writeDbEntry conferenceDay
                conferenceDay.Rooms 
                |> List.iter writeRoom

            let writeData (conferenceData : ConferenceData) =
                Database.writeDbEntry conferenceData
                conferenceData.Days 
                |> List.iter writeDay
             

            let sync conference (conferenceData : ConferenceData) =
                let currentConferenceData = Database.getConferenceData conference

                match currentConferenceData with
                | Some data ->
                    if data.Version <> conferenceData.Version then
                        Database.deleteConference conference
                        writeData conferenceData
                | _ ->
                    writeData conferenceData
        
        let getConferenceDays conference = 
            Database.getConferenceDays conference 
            |> Seq.toList

        let getEntriesForDay (day : ConferenceDay) =
            Database.getEntriesForDay day
            |> Seq.toList

        let getActualConferenceDays() =
            let conf = getActualConference()
            match conf with
            | Some conference ->
                getConferenceDays conference
            | _ ->
                List.empty

        let synchronizeData() =
            let conf = getActualConference()
            match conf with
            | Some conference ->
                let json = loadJsonFromUri conference.DataUri
                let conferenceData = Parser.parseJson conference json
                Synchronization.sync conference conferenceData
            | _ ->
                ignore()

    let Init(sqlitePlatform : SQLite.Net.Interop.ISQLitePlatform) =

        let extraTypeMappings = new Dictionary<Type, string>()
        
        extraTypeMappings.Add(typeof<NodaTime.LocalDate>, "blob")
        extraTypeMappings.Add(typeof<NodaTime.OffsetDateTime>, "blob")
        extraTypeMappings.Add(typeof<NodaTime.Duration>, "blob")

        let sqlConnection = new SQLiteConnection(sqlitePlatform, 
                                                    "ffrab.mobile.db", 
                                                    false,
                                                    NodaTypeSerializerDelegate.Delegate(),
                                                    null,
                                                    extraTypeMappings
                                                    )

        CurrentState <- new State(sqlConnection)

        if Conferences.Database.shouldCreateDatabase() then
            Conferences.Database.reCreateDatabase()
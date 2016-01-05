namespace ffrab.mobile.common

module Parser = 
    open System
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq
    open System.IO
    open common
    open entities

            
    let deserializeJson data = 
        let reader = new JsonTextReader(new StringReader(data))
        reader.DateParseHandling <- DateParseHandling.None
        JObject.Load(reader)
            
    let parseSpeaker (conferenceId : int) (speakerNode : JToken) =
        new Speaker(Id = speakerNode.["id"].Value<int>(), 
                    Name = speakerNode.["full_public_name"].Value<string>(), 
                    ConferenceId = conferenceId, 
                    Guid = Guid.NewGuid())

    let parseEntry (room : Room) (entryNode : JToken) = 
        let abstractText = entryNode.["abstract"].Value<string>() |> removeHTMLTags
        let description = entryNode.["description"].Value<string>() |> removeHTMLTags

        let entry = new Entry(Id = entryNode.["id"].Value<int>(), 
                                Guid = new Guid(entryNode.["guid"].Value<string>()), 
                                Title = entryNode.["title"].Value<string>(), 
                                Subtitle = entryNode.["subtitle"].Value<string>(), 
                                Abstract = abstractText, 
                                Track = entryNode.["track"].Value<string>(), 
                                Description = description, 
                                Start = common.Formatting.dateTimeFormat.Parse(entryNode.["date"].Value<string>()).Value, 
                                Language = entryNode.["language"].Value<string>(), 
                                Duration = common.Formatting.durationFormat.Parse(entryNode.["duration"].Value<string>()).Value, 
                                Type = entryNode.["type"].Value<string>(), 
                                ConferenceId = room.ConferenceId, 
                                ConferenceDayGuid = room.ConferenceDayGuid, 
                                RoomGuid = room.Guid)

        let speakers = entryNode.["persons"].Children()
                        |> List.ofSeq
                        |> List.map (fun x -> parseSpeaker room.ConferenceId x)

        entry.Speaker <- speakers

        entry
            
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
            
    let parseJson conference json = 
        match json with 
        | Some s ->
            deserializeJson s 
            |> parseSchedule conference
            |> Some
        | None ->
            None
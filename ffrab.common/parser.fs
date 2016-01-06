namespace ffrab.mobile.common

module Parser = 
    open System
    open common
    open entities
    open FSharp.Data

    type FrabJson = JsonProvider<"data-examples/ffrab1.json">
            
    let deserializeJson data = 
        FrabJson.Parse data
        
            
    let parseSpeaker (conferenceId : int) (speakerNode : JsonValue) =
        new Speaker(Id = speakerNode.["id"].AsInteger(), 
                    Name = speakerNode.["full_public_name"].AsString(), 
                    ConferenceId = conferenceId, 
                    Guid = Guid.NewGuid())

    let parseEntry (room : Room) (entryNode : JsonValue) = 

        let description = entryNode.["description"].AsString() |> removeHTMLTags

        let id = (entryNode.GetProperty "id").AsInteger()
        let guid = (entryNode.GetProperty "guid").AsGuid()
        let title = (entryNode.GetProperty "title").AsString()
        let subtitle = (entryNode.GetProperty "subtitle").AsString()
        let abstractText = entryNode.["abstract"].AsString() |> removeHTMLTags
        let track = (entryNode.GetProperty "track").AsString()
        let start = common.Formatting.dateTimeFormat.Parse(entryNode.["date"].AsString()).Value
        let language = entryNode.["language"].AsString()
        let duration = common.Formatting.durationFormat.Parse(entryNode.["duration"].AsString()).Value
        let entryType = entryNode.["type"].AsString()

        let entry = new Entry(Id = id, 
                                Guid = guid, 
                                Title = title, 
                                Subtitle = subtitle, 
                                Abstract = abstractText, 
                                Track = track, 
                                Description = description, 
                                Start = start, 
                                Language = language, 
                                Duration = duration, 
                                Type = entryType, 
                                ConferenceId = room.ConferenceId, 
                                ConferenceDayGuid = room.ConferenceDayGuid, 
                                RoomGuid = room.Guid)

        let persons = (entryNode.GetProperty "persons").AsArray()
        let speakers = persons
                        |> Seq.map (fun x -> parseSpeaker room.ConferenceId x)
                        |> List.ofSeq

        entry.Speaker <- speakers

        entry
            
    let parseRoom (conferenceDay : ConferenceDay) (name : string) (talks : JsonValue) = 
        let array = talks.AsArray()
        let room = 
            new Room(Name = name, Guid = Guid.NewGuid(), 
                        ConferenceId = conferenceDay.ConferenceId, ConferenceDayGuid = conferenceDay.Guid)
                
        let entries = 
            array
            |> Seq.map (fun x -> parseEntry room x)
            |> List.ofSeq
        room.Entries <- entries
        room
            
    let parseDay (conference : Conference) (dayNode : FrabJson.Day) = 
        let json = dayNode.JsonValue
        

        let date = json.GetProperty "date"
        let day = common.Formatting.dateFormat.Parse(date.AsString()).Value


        let startTime = common.Formatting.dateTimeFormat.Parse((json.GetProperty "day_start").AsString()).Value
        let endTime = common.Formatting.dateTimeFormat.Parse((json.GetProperty "day_end").AsString()).Value
        let conferenceDay = 
            new ConferenceDay(Index = dayNode.Index, Day = day, StartTime = startTime, 
                                EndTime = endTime, ConferenceId = conference.Id, Guid = Guid.NewGuid())
          
                
        let rooms = 
            dayNode.Rooms.JsonValue.Properties()
            |> Seq.map (fun (name, talks) -> parseRoom conferenceDay name talks)
            |> List.ofSeq

        conferenceDay.Rooms <- rooms
        conferenceDay
            
    let parseDays conference (conferenceNode : FrabJson.Conference) = 
        let daysNode = conferenceNode.Days
        daysNode
        |> List.ofSeq
        |> List.map (fun x -> parseDay conference x)
            
    let parseSchedule (conference : Conference) (root : FrabJson.Root) = 
        let scheduleNode = root.Schedule
        let conferenceNode = scheduleNode.Conference
        let version = scheduleNode.Version
        let days = parseDays conference conferenceNode
        new ConferenceData(Version = version, Days = days, ConferenceId = conference.Id)
            
    let parseJson conference json = 
        match json with 
        | Some s ->
            deserializeJson s 
            |> parseSchedule conference
            |> Some
        | None ->
            None
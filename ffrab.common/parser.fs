namespace www.linuxwochen.common

module Parser = 
    open System
    open common
    open entities
    open FSharp.Data

    type FrabJson = JsonProvider<"data-examples/ffrab1.json">
            
    let parseNodaTime<'T> (pattern : NodaTime.Text.IPattern<'T>) rawValue =
        let result = pattern.Parse(rawValue)
        result.Value

    let deserializeJson data = 
        FrabJson.Parse data
                    
    let parseSpeaker (conferenceId : int) (speakerNode : JsonValue) =
        new Speaker(Id = speakerNode.["id"].AsInteger(), 
                    Name = speakerNode.["public_name"].AsString(), 
                    ConferenceId = conferenceId, 
                    Guid = Guid.NewGuid())

    let parseEntry (room : Room) (entryNode : JsonValue) = 

        let id = (entryNode.GetProperty "id").AsInteger()
        let guid = (entryNode.GetProperty "guid").AsGuid()
        let title = (entryNode.GetProperty "title").AsString()
        let subtitle = (entryNode.GetProperty "subtitle").AsString()
        let abstractText = entryNode.["abstract"].AsString() |> removeHTMLTags
        let track = (entryNode.GetProperty "track").AsString()
        let description = entryNode.["description"].AsString() |> removeHTMLTags
        let start = entryNode.["date"].AsString() |> parseNodaTime common.Formatting.dateTimeFormat
        let language = entryNode.["language"].AsString()
        let duration = entryNode.["duration"].AsString() |> parseNodaTime common.Formatting.durationFormat
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

        entry.Speaker <- (entryNode.GetProperty "persons").AsArray()
                        |> Seq.map (fun x -> parseSpeaker room.ConferenceId x)
                        |> List.ofSeq
        entry
            
    let parseRoom (conferenceDay : ConferenceDay) (name : string) (talks : JsonValue) = 
        let room = new Room(Name = name, 
                            Guid = Guid.NewGuid(), 
                            ConferenceId = conferenceDay.ConferenceId, 
                            ConferenceDayGuid = conferenceDay.Guid)
        
        room.Entries <- talks.AsArray()
                        |> Seq.map (fun x -> parseEntry room x)
                        |> List.ofSeq
        room
            
    let parseDay (conference : Conference) (dayNode : FrabJson.Day) = 
        let json = dayNode.JsonValue
        
        let day = (json.GetProperty "date").AsString() |> parseNodaTime common.Formatting.dateFormat
        let startTime = (json.GetProperty "day_start").AsString() |> parseNodaTime common.Formatting.dateTimeFormat
        let endTime = (json.GetProperty "day_end").AsString() |> parseNodaTime common.Formatting.dateTimeFormat

        let conferenceDay = new ConferenceDay(Index = dayNode.Index, 
                                              Day = day, 
                                              StartTime = startTime, 
                                              EndTime = endTime, 
                                              ConferenceId = conference.Id, 
                                              Guid = Guid.NewGuid())
          
                
        let rooms = 
            dayNode.Rooms.JsonValue.Properties()
            |> Seq.map (fun (name, talks) -> parseRoom conferenceDay name talks)
            |> List.ofSeq

        conferenceDay.Rooms <- rooms
        conferenceDay
            
    let parseDays conference (conferenceNode : FrabJson.Conference) = 
        conferenceNode.Days
        |> List.ofSeq
        |> List.map (fun x -> parseDay conference x)
            
    let parseSchedule (conference : Conference) (root : FrabJson.Root) = 
        let scheduleNode = root.Schedule
        let conferenceNode = scheduleNode.Conference
        let days = parseDays conference conferenceNode
        new ConferenceData(Version = scheduleNode.Version, 
                          Days = days, 
                          ConferenceId = conference.Id)
            
    let parseJson conference json = 
        match json with 
        | Some s ->
            deserializeJson s 
            |> parseSchedule conference
            |> Some
        | None ->
            None
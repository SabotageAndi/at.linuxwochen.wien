namespace ffrab.mobile.common


module Synchronization =
    open entities
    open NodaTime
    open queries

    let writeSpeaker (speaker : Speaker) (entry : Entry)=
        let existingSpeaker = getSpeakerForConference speaker.Id speaker.ConferenceId

        let actualSpeaker = match existingSpeaker with
                            | Some(_) -> existingSpeaker.Value
                            | _ ->
                                Database.writeDbEntry speaker
                                speaker
        let speaker2Entry = new Speaker2Entry(EntryGuid = entry.Guid, SpeakerGuid = actualSpeaker.Guid, ConferenceId = speaker.ConferenceId)
        Database.writeDbEntry speaker2Entry

    let writeEntry (entry : Entry) =
        Database.writeDbEntry entry

        entry.Speaker
        |> List.iter (fun speaker -> writeSpeaker speaker entry)

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
             
    let writeLastSync (conferenceData : ConferenceData) =
        conferenceData.LastSync <- SystemClock.Instance.Now.WithOffset(Offset.Zero)
        Database.updateDbEntry conferenceData
        ignore()


    let sync conference (conferenceData : ConferenceData option) =
        match conferenceData with
        | Some conferenceData ->
            let currentConferenceData = getConferenceData conference

            match currentConferenceData with
            | Some data ->
                if data.Version <> conferenceData.Version then
                    deleteConference conference
                    writeData conferenceData
                writeLastSync conferenceData
            | _ ->
                writeData conferenceData
                writeLastSync conferenceData
        | None ->
            ignore()


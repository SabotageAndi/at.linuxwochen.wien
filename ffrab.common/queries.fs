namespace ffrab.mobile.common


module queries =
    open System
    open entities
    open Database

    let isEntryFavorite (entry : Entry) =            
        filter<EntryFavorite> <@ fun (ef : EntryFavorite) -> (ef.ConferenceId = entry.ConferenceId && ef.EntryId = entry.Id) @> 
        |> any

    let setEntryFavorite (entryFavorite : EntryFavorite) =
        writeDbEntry entryFavorite

    let removeEntryFavorite (entry : Entry) =
        filter<EntryFavorite> <@ fun ef -> ef.ConferenceId = entry.ConferenceId && ef.EntryId = entry.Id @>
        |> delete

    let deleteConference (conference : Conference) =
        filter<Entry> <@ fun e -> e.ConferenceId = conference.Id @> |> delete
        filter<Room> <@ fun e -> e.ConferenceId = conference.Id @> |> delete
        filter<ConferenceDay> <@ fun e -> e.ConferenceId = conference.Id @> |> delete
        filter<ConferenceData> <@ fun e -> e.ConferenceId = conference.Id @> |> delete
        filter<Speaker> <@ fun e -> e.ConferenceId = conference.Id @> |> delete
        filter<Speaker2Entry> <@ fun e -> e.ConferenceId = conference.Id @> |> delete

    let getConferenceData (conference : Conference) =
        filter<ConferenceData> <@ fun c -> c.ConferenceId = conference.Id @>
        |> tryFirst

    let getSpeaker (speakerGuid : Guid) =
        filter<Speaker> <@ fun s -> s.Guid = speakerGuid @>
        |> tryFirst

    let getSpeakerForConference (speakerId : int) (conferenceId : int) =
        filter<Speaker> <@ fun s -> s.Id = speakerId && s.ConferenceId = conferenceId @>
        |> tryFirst

    let getSpeakersOfEntry (entry : Entry) =
        filter<Speaker2Entry> <@ fun s2e -> s2e.EntryGuid = entry.Guid @>
        |> Seq.map (fun se -> getSpeaker(se.SpeakerGuid))
        |> Seq.filter(fun s ->
            match s with
            | Some(_) -> true
            | _ -> false)
        |> Seq.map (fun s -> s.Value)
        |> Seq.toList

    let getConferenceDays (conference : Conference) = 
        filter<ConferenceDay> <@ fun cd -> cd.ConferenceId = conference.Id @>
        |> Seq.toList

    let getEntriesForDay (day : ConferenceDay) =
        filter<Entry> <@ (fun e -> e.ConferenceDayGuid = day.Guid) @>
        |> Seq.map (fun e -> 
            e.Speaker <- getSpeakersOfEntry(e)
            e)
        |> Seq.sortBy (fun e -> e.Start.ToDateTimeOffset())
        |> Seq.toList
    
    let getRoom roomGuid =
        filter<Room> <@ (fun r -> r.Guid = roomGuid) @>
        |> tryFirst

    let getTopFavorites number (conf : Conference option) =
        match conf with
        | Some conference ->
            let sql = sprintf "select Entry.* from Entry 
                                inner join EntryFavorite on EntryFavorite.ConferenceId = Entry.ConferenceId and EntryFavorite.EntryId = Entry.Id 
                                where Entry.ConferenceId = ? order by Entry.Start asc limit %i" number
                    
            let entries = Database.query<Entry>(sql, conference.Id)

            entries
            |> List.ofSeq
        | _ ->
            List.Empty
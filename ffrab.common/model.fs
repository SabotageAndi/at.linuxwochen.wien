namespace ffrab.mobile.common

module model = 
    open Xamarin.Forms
    open System
    open SQLite.Net
    open entities
    open common
    open NodaTime
    open System.Collections.Generic

    
    module Conferences = 

        open Database
        open queries

        
        type UriType =
        | Http
        | Local

        let actualConfKey = "actualConference"
        
        let conferences =
            [// new Conference(1, "Vienna Mobile Quality Night", "file://data/vmqn2015.json", constants.vmqnJson)
              new Conference(2, "Linuxwochen Wien 2015", "https://cfp.linuxwochen.at/en/LWW15/public/schedule.json", "") ]
        
        let getAllConferences() = conferences
        let getConference id = 
            let conf = conferences 
                       |> List.tryFind (fun i -> i.Id = id)

            match conf with
            | Some conf ->
                conf.Data <- getConferenceData conf
            | None ->
                ignore()
            conf

        let setActualConference (conf : Conference) = 
            Application.Current.Properties.[actualConfKey] <- conf.Id
        
        let getActualConference() = 
            getConference 2 //only enable Linuxwochen 2015
//            let keyExists = Application.Current.Properties.ContainsKey(actualConfKey)
//            match keyExists with
//            | true -> 
//                let id = Application.Current.Properties.[actualConfKey] :?> int
//                getConference id
//            | _ -> None
   
               
        let getActualConferenceDays() =
            let conf = getActualConference()
            match conf with
            | Some conference ->
                getConferenceDays conference
            | _ ->
                List.empty

        let getDataLocation (conf : Conference option) =
            match conf with
            | Some conf ->
                let uri = conf.DataUri
                let start = uri.Substring(0, 4)

                if start = "http" then 
                    (Some UriType.Http, Some conf)
                else
                    (Some UriType.Local, Some conf)
            | None ->
                (None, None)

        let fetchJson (uriType : UriType option, conf : Conference option) =
            match conf with
            | None ->
                None
            | Some conf ->
                match uriType with
                | Some UriType.Http ->
                    loadJsonFromUri conf.DataUri
                    |> Some
                | Some UriType.Local ->
                    Some conf.RawData
                | None ->
                    None

        let checkForTimeout (conf : Conference)  =
            match conf.Data with
            | Some confData ->
                let durationSinceLastSync = NodaTime.SystemClock.Instance.Now - confData.LastSync.ToInstant()
                match (durationSinceLastSync.ToTimeSpan().TotalMinutes < 15.0) with
                | true -> None
                | false -> Some conf
            | None ->
                Some conf

        let synchronizeData conference =
            match conference with
            | Some conference ->
                conference 
                |> checkForTimeout
                |> getDataLocation
                |> fetchJson 
                |> Parser.parseJson conference
                |> Synchronization.sync conference
            | _ ->
                ignore()
            conference

    module Entry =

        let isEntryFavorite (entry : Entry) =
            queries.isEntryFavorite entry

        let toggleEntryFavorite (entry : Entry) =
            match isEntryFavorite entry with 
            | true ->
                queries.removeEntryFavorite entry
            | false ->
                let entryFavorite = new EntryFavorite(Guid = Guid.NewGuid(), EntryId = entry.Id, ConferenceId = entry.ConferenceId)
                queries.setEntryFavorite entryFavorite
            

    let Init (sqlitePlatform : SQLite.Net.Interop.ISQLitePlatform, databaseFilePath : string) =

        let extraTypeMappings = new Dictionary<Type, string>()
        
        extraTypeMappings.Add(typeof<NodaTime.LocalDate>, "blob")
        extraTypeMappings.Add(typeof<NodaTime.OffsetDateTime>, "blob")
        extraTypeMappings.Add(typeof<NodaTime.Duration>, "blob")

        let sqlConnection = new SQLiteConnection(sqlitePlatform, 
                                                    databaseFilePath, 
                                                    false,
                                                    NodaTypeSerializerDelegate.Delegate(),
                                                    null,
                                                    extraTypeMappings
                                                    )

        CurrentState <- new State(sqlConnection)

        Database.createSchema()

    
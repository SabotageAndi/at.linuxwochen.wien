module Synchronization

    open Xunit
    open FsUnit.Xunit
    open System
    open System.IO
    open ffrab.mobile.common.model
    open ffrab.mobile.common.entities

    let conference = new Conference(1, "", "", "")

    let parseJson() = 
        File.ReadAllText("data/conference.json") |> Conferences.Parser.parseJson conference

    let init() =
        let db = new SQLite.Net.Platform.Win32.SQLitePlatformWin32()
        Init(db)
        Conferences.Database.reCreateDatabase()
        CurrentState

    [<Fact>]
    let ``Synchronate test data into empty database``() =
        let currentState = init()
        let conferenceData = parseJson()
        Conferences.Synchronization.sync conference conferenceData

        currentState.SQLConnection.Table<ConferenceData>().Count() |> should equal 1
        currentState.SQLConnection.Table<ConferenceDay>().Count() |> should equal 3
        currentState.SQLConnection.Table<Room>().Count() |> should equal 21
        currentState.SQLConnection.Table<Entry>().Count() |> should equal 101

    [<Fact>]
    let ``Synchronate test data into filled database with old version``() =
        let currentState = init()
        let conferenceData = parseJson()

        let oldConferenceData = new ConferenceData(ConferenceId = conference.Id, Version= "0")
        currentState.SQLConnection.Insert oldConferenceData |> ignore

        Conferences.Synchronization.sync conference conferenceData

        currentState.SQLConnection.Table<ConferenceData>().Count() |> should equal 1
        currentState.SQLConnection.Table<ConferenceDay>().Count() |> should equal 3
        currentState.SQLConnection.Table<Room>().Count() |> should equal 21
        currentState.SQLConnection.Table<Entry>().Count() |> should equal 101

    [<Fact>]
    let ``Not Synchronate test data into filled database with current version``() =
        let currentState = init()
        let conferenceData = parseJson()

        let oldConferenceData = new ConferenceData(ConferenceId = conference.Id, Version= "2.0.4")
        currentState.SQLConnection.Insert oldConferenceData |> ignore

        Conferences.Synchronization.sync conference conferenceData

        currentState.SQLConnection.Table<ConferenceData>().Count() |> should equal 1
        currentState.SQLConnection.Table<ConferenceDay>().Count() |> should equal 0
        currentState.SQLConnection.Table<Room>().Count() |> should equal 0
        currentState.SQLConnection.Table<Entry>().Count() |> should equal 0
module Database 

    open Xunit
    open FsUnit.Xunit
    open System
    open System.IO
    open SQLite.Net
    open ffrab.mobile.common.model
    open ffrab.mobile.common.entities

    let conf = new Conference(1, "", "")

    let init() =
        let dbpath = Path.GetTempFileName()
        let db = new SQLite.Net.Platform.Win32.SQLitePlatformWin32()
        Init(db)
        ffrab.mobile.common.model.CurrentState.SQLConnection

    let initWithRecreate() =
        let db = init()
        Conferences.Database.reCreateDatabase()
        db

    [<Fact>]
    let ``Database is recreated``() =
        let db = init()
        Conferences.Database.reCreateDatabase()
    
    [<Fact>]   
    let ``Entries removed on conference clean``() =
        let db = initWithRecreate()

        let entry = new Entry(ConferenceId = 1)
        db.Insert entry |> should equal 1

        Conferences.Database.deleteConference conf


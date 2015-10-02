module Parsing
    open Xunit
    open FsUnit.Xunit
    open System.IO
    open ffrab.mobile.common.model
    open NodaTime
    
    let parseJson() = 
        let conf = new Conference(1, "", "", "")
        File.ReadAllText("data/conference.json") |> Conferences.Parser.parseJson conf
    
    [<Fact>]
    let ``get correct number of days``() = 
        let data = parseJson()
        data.Days.Length |> should equal 3
    
    [<Fact>]
    let ``Get Version number of schedule``() = 
        let data = parseJson()
        data.Version |> should equal "2.0.4"
    
    [<Fact>]
    let ``Get correct dates for days``() = 
        let data = parseJson()
        data.Days.Item(0).Day |> should equal (new LocalDate(2015, 5, 7))
        data.Days.Item(1).Day |> should equal (new LocalDate(2015, 5, 8))
        data.Days.Item(2).Day |> should equal (new LocalDate(2015, 5, 9))
    
    [<Fact>]
    let ``Get start time of day``() = 
        let data = parseJson()
        data.Days.Item(0).StartTime 
        |> should equal (new OffsetDateTime(new LocalDateTime(2015, 5, 7, 10, 0, 0), Offset.FromHours(2)))
    
    [<Fact>]
    let ``Get end time of day``() = 
        let data = parseJson()
        data.Days.Item(0).EndTime 
        |> should equal (new OffsetDateTime(new LocalDateTime(2015, 5, 7, 20, 0, 0), Offset.FromHours(2)))

    [<Fact>]
    let ``Parse rooms``() =
        let data = parseJson()
        data.Days.Item(0).Rooms.Length |> should equal 7

    [<Fact>]
    let ``Parse room name``() =
        let data = parseJson()
        let room = data.Days.Item(0).Rooms.Item(0)
        room.Name 
        |> should equal "FS 0.01"

    [<Fact>]
    let ``Parse entry``() = 
        let data = parseJson()
        let entry = data.Days.Item(0).Rooms.Item(0).Entries.Item(0)

        entry.Id 
        |> should equal 311

        entry.Guid
        |> should equal "38a1c2da-bb3e-47f8-b832-945b8d5ab1ab"

        entry.Title
        |> should equal "Begrüßung"

        entry.Abstract
        |> should equal ""

        entry.Description
        |> should equal ""

        entry.Language
        |> should equal "de"

        entry.Start
        |> should equal (new OffsetDateTime(new LocalDateTime(2015, 5, 7, 10, 30, 0), Offset.FromHours(2)))

        entry.Subtitle
        |> should equal ""

        entry.Track
        |> should equal "Open Everything"

        entry.Type
        |> should equal "lecture"

    [<Fact>]
    let ``correct conference id in day``() =
        let data = parseJson()

        data.Days.Item(0).ConferenceId
        |> should equal data.ConferenceId

    [<Fact>]
    let ``correct ids in room``() =
        let data = parseJson()

        let day = data.Days.Item(0)

        day.Rooms.Item(0).ConferenceId
        |> should equal data.ConferenceId

        day.Rooms.Item(0).ConferenceDayGuid
        |> should equal day.Guid

    [<Fact>]
    let ``correct ids in entry``() =
        let data = parseJson()

        let day = data.Days.Item(0)
        let room = day.Rooms.Item(0)
        let entry = room.Entries.Item(0)

        entry.ConferenceId
        |> should equal data.ConferenceId

        entry.ConferenceDayGuid 
        |> should equal day.Guid

        entry.RoomGuid
        |> should equal room.Guid

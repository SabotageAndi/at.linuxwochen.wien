namespace ffrab.mobile.test

open Xunit
open FsUnit.Xunit
open System
open System.IO
open FSharp.Data

module dataloading = 
    open ffrab.mobile.common.model
    open NodaTime
    
    let parseJson() = File.ReadAllText("data/conference.json") |> Conferences.parseJson
    
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

namespace ffrab.mobile.test

open Xunit
open FsUnit.Xunit
open System.IO

open FSharp.Data

module dataloading = 

    open ffrab.mobile.common.model

    [<Fact>]
    let ``get correct number of days``() =
        let rawdata = File.ReadAllText("data/conference.json")

        let data = Conferences.parseJson rawdata

        data.Days.Length |> should equal 3
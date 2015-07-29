namespace ffrab.mobile.common


module model =

    open Xamarin.Forms
    open System

    type ConferenceDay(day) =
        let day = day

        member this.Day with get() = day

    type ConferenceData()=
        let mutable days : ConferenceDay list = []
        member this.Days with get() = days

        member this.addDay day =
            days <- day :: days

    [<AllowNullLiteral>]
    type Conference(id, name, dataUri) =
        let id = id
        let name = name
        let dataUri = dataUri
        let mutable data : ConferenceData option = None
        
        member this.Name with get() = name
        member this.Id with get() = id
        member this.DataUri with get() = dataUri
        member this.Data with get() = data and set(v) = data <- v
        


    module Conferences =
        let actualConfKey = "actualConference"

        let conferences = 
            [
                new Conference(1, "Linuxwochen 2013", "https://cfp.linuxwochen.at/en/LWW13/public/schedule.json")
                new Conference(2, "Linuxwochen 2014", "https://cfp.linuxwochen.at/en/LWW14/public/schedule.json")
                new Conference(3, "Linuxwochen 2015", "https://cfp.linuxwochen.at/en/LWW15/public/schedule.json") 
            ]

        let getAllConferences() =
            conferences


        let getConference id =
            conferences |> List.tryFind (fun i -> i.Id = id)

        let setActualConference (conf : Conference) =
            Application.Current.Properties.[actualConfKey] <- conf.Id

        let getActualConference () =
            let keyExists = Application.Current.Properties.ContainsKey(actualConfKey)
            
            match keyExists with
            | true ->
                let id = Application.Current.Properties.[actualConfKey] :?> int
                getConference id
            | _ ->
                None

        let getConferenceData (conf : Conference)=
            match conf.Data with
            | Some data ->
                data
            | _ ->
                let data = new ConferenceData()
                new ConferenceDay(DateTime.Today) |> data.addDay
                new ConferenceDay(DateTime.Today.AddDays(-1.0)) |> data.addDay
                new ConferenceDay(DateTime.Today.AddDays(-2.0)) |> data.addDay
                conf.Data <- Some data
                data
                
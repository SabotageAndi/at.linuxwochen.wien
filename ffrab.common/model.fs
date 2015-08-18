namespace ffrab.mobile.common


module model =

    open Xamarin.Forms
    open System
    open FSharp.Data

    type ConferenceDay(day : DateTime) =
        let day = day

        member this.Day with get() = day

    type ConferenceData()=
        let mutable days : ConferenceDay list = []
        member this.Days with get() = days

        member this.AddDay day =
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

        open Newtonsoft.Json
        open Newtonsoft.Json.Linq
        open System.IO
        open System.Linq
        

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

        let loadJsonFromUri (uri : string)  =
            async {
                let httpClient = new System.Net.Http.HttpClient()
                let! response = httpClient.GetAsync(uri) |> Async.AwaitTask
                response.EnsureSuccessStatusCode () |> ignore
                let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
                return content
            } |> Async.RunSynchronously

        let deserializeJson data =
            JObject.Parse data

        let parseSchedule (root : JObject) =
            let scheduleNode = root.["schedule"]
            let conferenceNode = scheduleNode.["conference"] :?> JObject
            
            (new ConferenceData(), conferenceNode)

        let parseDay (dayNode : JToken) =
            new ConferenceDay(DateTime.Now)

        let parseDays (confData : ConferenceData, conferenceNode : JObject) =
            let daysNode = conferenceNode.["days"]

            daysNode.Children().ToArray() |>
            List.ofArray |>
            List.map parseDay |>
            List.iter (fun i -> confData.AddDay(i))

            confData

        let parseJson json =
            let root = deserializeJson json

            let conferenceData = root |>
                                 parseSchedule |>
                                 parseDays


            conferenceData
            
        let getConferenceData (conf : Conference)=
            match conf.Data with
            | Some data ->
                data
            | _ ->
                let data = loadJsonFromUri conf.DataUri |>
                           parseJson

                conf.Data <- Some data
                data
                
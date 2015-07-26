namespace ffrab.mobile.common

open Xamarin.Forms

module model =

    type Conference(id, name, dataUri) =
        let id = id
        let name = name
        let dataUri = dataUri

        member this.Name with get() = name
        member this.Id with get() = id
        member this.DataUri with get() = dataUri


    module Conferences =
        let conferences = 
            [
                new Conference(1, "Linuxwochen 2013", "https://cfp.linuxwochen.at/en/LWW13/public/schedule.json")
                new Conference(2, "Linuxwochen 2014", "https://cfp.linuxwochen.at/en/LWW14/public/schedule.json")
                new Conference(3, "Linuxwochen 2015", "https://cfp.linuxwochen.at/en/LWW15/public/schedule.json") 
            ]

        let getAllConferences() =
            conferences


        let getConference id =
            conferences |> List.find (fun i -> i.Id = id)

        let setActualConference (conf : Conference) =
            Application.Current.Properties.["actualConference"] <- conf.Id

        let getActualConference () =
            let id = Application.Current.Properties.["actualConference"] :?> int
            getConference id
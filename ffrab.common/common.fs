namespace ffrab.mobile.common

module common =

    let (|As|_|) (p:'T) : 'U option =
            let p = p :> obj
            if p :? 'U then Some (p :?> 'U) else None

    type ViewModelType =
        | About
        | ConferenceList 
        | Main

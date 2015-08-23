namespace ffrab.mobile.common

open System

module common = 
    let (|As|_|) (p : 'T) : 'U option = 
        let p = p :> obj
        if p :? 'U then Some(p :?> 'U)
        else None
    
    type ViewModelType = 
        | About
        | ConferenceList
        | Main
        | Day of NodaTime.LocalDate

    open NodaTime
    open SQLite.Net

    module NodaTypeSerializerDelegate =

        let supportedTypes = [
            typeof<NodaTime.LocalDate>;
            typeof<NodaTime.OffsetDateTime>;
            typeof<NodaTime.Duration>
            ]

        let getSupportedType typ =
            supportedTypes
            |> List.filter (fun i -> i = typ)
            |> List.tryHead

        let canSerialize (typ : Type) =
            let supportedTyp = getSupportedType typ

            match typ with
            | supportedTyp ->
                true
            | _ ->
                false

        let serialize (obj : Object) =
            Array.init<byte> 0 (fun index -> 0uy)
            


        let deserialize (data : byte[]) (typ : Type) =
            new Object()

        let Delegate() =
            new BlobSerializerDelegate(new BlobSerializerDelegate.SerializeDelegate(serialize),
                                       new BlobSerializerDelegate.DeserializeDelegate(deserialize),
                                       new BlobSerializerDelegate.CanSerializeDelegate(canSerialize))
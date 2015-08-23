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
    open NodaTime.Text
    open SQLite.Net

    open System.Runtime.Serialization
    open System.Runtime.Serialization.Formatters

    module NodaTypeSerializerDelegate =

        type SupportedTyp = {
            Typ : Type;
        }

        let localDateType = typeof<LocalDate>
        let offsetDateTimeType = typeof<OffsetDateTime>
        let durationType = typeof<Duration>

        let supportedTypes = [
            (localDateType);
            (offsetDateTimeType);
            (durationType)
            ]

        let getSupportedType typ =
            supportedTypes
            |> List.filter (fun i -> i = typ)
            |> List.tryHead

        let canSerialize (typ : Type) =
            let supportedTyp = getSupportedType typ

            match supportedTyp with
            | Some x ->
                true
            | _ ->
                false

        

        let serialize (obj : Object) =
            
            let typ = obj.GetType()
            
            let mutable formatResult = ""

            if typ = localDateType then
                formatResult <- LocalDatePattern.IsoPattern.Format (obj :?> LocalDate)

            if typ = offsetDateTimeType then
                formatResult <- OffsetDateTimePattern.ExtendedIsoPattern.Format (obj :?> OffsetDateTime)
                
            if typ = durationType then
                formatResult <- DurationPattern.RoundtripPattern.Format (obj :?> Duration)
                
            System.Text.Encoding.Unicode.GetBytes formatResult

            
        let deserialize (data : byte[]) (typ : Type) : Object =
            let text = System.Text.Encoding.Unicode.GetString(data, 0, data.Length)

            let mutable result : Object = null

            if typ = localDateType then
                let parseResult = LocalDatePattern.IsoPattern.Parse text
                result <- parseResult.Value :> Object

            if typ = offsetDateTimeType then
                let parseResult = OffsetDateTimePattern.ExtendedIsoPattern.Parse text
                result <- parseResult.Value :> Object

            if typ = durationType then
                let parseResult = DurationPattern.RoundtripPattern.Parse text
                result <- parseResult.Value :> Object

            result
            

        let Delegate() =
            new BlobSerializerDelegate(new BlobSerializerDelegate.SerializeDelegate(serialize),
                                       new BlobSerializerDelegate.DeserializeDelegate(deserialize),
                                       new BlobSerializerDelegate.CanSerializeDelegate(canSerialize))
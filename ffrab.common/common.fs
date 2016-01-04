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

    let runOnUIthread func =
        let action = new Action(func)
        Xamarin.Forms.Device.BeginInvokeOnMainThread action

         
    let loadJsonFromUri (uri : string) = 
        async { 
            let httpClient = new System.Net.Http.HttpClient()
            let! response = httpClient.GetAsync(uri) |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            return content
        }
        |> Async.RunSynchronously

    open NodaTime
    open NodaTime.Text
    open SQLite.Net

    module Formatting =
        let dateFormat = LocalDatePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd")
        let dateTimeFormat = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'sso<G>")
        let durationFormat = DurationPattern.CreateWithInvariantCulture("H:ss")
        let durationOffsetFormat = OffsetDateTimePattern.CreateWithInvariantCulture("H:ss")

    module NodaTypeSerializerDelegate =


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
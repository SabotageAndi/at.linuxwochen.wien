namespace www.linuxwochen.common

open System
open System.Text.RegularExpressions

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

    let removeHTMLTags str =
        Regex.Replace(str, @"<[^>]+>|&nbsp;", "").Trim();
         
    let loadJsonFromUri (uri : string) = 
        async { 
            let httpClient = new System.Net.Http.HttpClient()
            let! response = httpClient.GetAsync(uri) |> Async.AwaitTask
            response.EnsureSuccessStatusCode() |> ignore
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            return content
        }
        |> Async.RunSynchronously

    open System.Linq.Expressions
    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter
    
    let toLinq (expr : Expr<'a -> 'b>) =
      let linq = expr |> QuotationToExpression
      let call = linq :?> MethodCallExpression
      let lambda = call.Arguments.[0] :?> LambdaExpression
      Expression.Lambda<Func<'a, 'b>>(lambda.Body, lambda.Parameters) 

    type State(conn) = 
        let sqlConnection : SQLite.Net.SQLiteConnection = conn

        member this.SQLConnection 
            with get() = sqlConnection
    

    let mutable CurrentState : State = new State(null)

    open NodaTime
    open NodaTime.Text  
    open SQLite.Net


    let now() =
        let now = NodaTime.SystemClock.Instance.Now
        let timeback = now.Minus(Duration.FromStandardDays(332L)).Minus(Duration.FromHours(4L))

        timeback.InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).ToOffsetDateTime()

    module Formatting =
        let dateFormat = LocalDatePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd")
        let dateTimeFormat = OffsetDateTimePattern.CreateWithInvariantCulture("yyyy'-'MM'-'dd'T'HH':'mm':'sso<G>")
        let durationFormat = DurationPattern.CreateWithInvariantCulture("H:ss")
        let timeOffsetFormat = OffsetDateTimePattern.CreateWithInvariantCulture("H:ss")
        let dateOffsetFormat = OffsetDateTimePattern.CreateWithInvariantCulture("dd.MM")
        let shortdateFormat = LocalDatePattern.CreateWithInvariantCulture("dd'.'MM")


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
                
            System.Text.Encoding.UTF8.GetBytes formatResult

            
        let deserialize (data : byte[]) (typ : Type) : Object =
            let text = System.Text.Encoding.UTF8.GetString(data, 0, data.Length)

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
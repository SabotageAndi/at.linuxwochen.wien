module infrastructure

    open Xunit
    open FsUnit.Xunit
    open NodaTime


    module NodaTypeSerializerDelegate =

        open www.linuxwochen.common.common

        [<Fact>]
        let ``can serialize LocalDate``() =
            let serializeable = NodaTypeSerializerDelegate.canSerialize typeof<LocalDate>

            serializeable 
            |> should equal true

        [<Fact>]
        let ``can serialize Duration``() =
            let serializeable = NodaTypeSerializerDelegate.canSerialize typeof<Duration>

            serializeable 
            |> should equal true

        [<Fact>]
        let ``can serialize OffsetDateTime``() =
            let serializeable = NodaTypeSerializerDelegate.canSerialize typeof<OffsetDateTime>

            serializeable 
            |> should equal true

        [<Fact>]
        let ``serialize LocalDate``() =
            let localDate = new LocalDate(2015, 8, 24)

            let serialized = NodaTypeSerializerDelegate.serialize localDate

            //2015-08-24
            let expectedResult : byte[] = [|50uy; 0uy; 48uy; 0uy; 49uy; 0uy; 53uy; 0uy; 45uy; 0uy; 48uy; 0uy; 56uy; 0uy; 45uy; 0uy; 50uy; 0uy; 52uy; 0uy;|]

            serialized 
            |> should equal expectedResult

        [<Fact>]
        let ``serialize OffsiteDateTime``() = 
            let offsiteDateTime = new OffsetDateTime(new LocalDateTime(2015, 8, 24, 18, 06, 32, 678, 345), Offset.FromHoursAndMinutes(2,30))

            let serialized = NodaTypeSerializerDelegate.serialize offsiteDateTime

            //2015-08-24T18:06:32.6780345+02:30
            let expectedResult : byte[] =  [|50uy; 0uy; 48uy; 0uy; 49uy; 0uy; 53uy; 0uy; 45uy; 0uy; 48uy; 0uy; 56uy; 0uy; 45uy; 0uy; 50uy; 0uy; 52uy; 0uy;
                                             84uy; 0uy; 
                                             49uy; 0uy; 56uy; 0uy; 58uy; 0uy; 48uy; 0uy; 54uy; 0uy; 58uy; 0uy; 51uy; 0uy; 50uy; 0uy;
                                             46uy; 0uy;
                                             54uy; 0uy; 55uy; 0uy; 56uy; 0uy; 48uy; 0uy; 51uy; 0uy; 52uy; 0uy; 53uy; 0uy;
                                             43uy; 0uy; 48uy; 0uy; 50uy; 0uy; 58uy; 0uy; 51uy; 0uy; 48uy; 0uy;|]

            serialized 
            |> should equal expectedResult

        [<Fact>]
        let ``serialize Duration``() =
            let seconds : int64 = 2L * 24L * 60L * 60L + 134L * 60L + 43L
            let duration = Duration.FromSeconds seconds

            let serialized = NodaTypeSerializerDelegate.serialize duration

            //2:02:34:00
            let expectedResult : byte[] = [|50uy; 0uy; 58uy; 0uy; 48uy; 0uy; 50uy; 0uy; 58uy; 0uy; 49uy; 0uy; 52uy; 0uy; 58uy; 0uy; 52uy; 0uy; 51uy; 0uy; |]

            serialized
            |> should equal expectedResult
           
        [<Fact>]
        let ``deserialize LocalDate``() =
            //2015-08-24
            let input : byte[] = [|50uy; 0uy; 48uy; 0uy; 49uy; 0uy; 53uy; 0uy; 45uy; 0uy; 48uy; 0uy; 56uy; 0uy; 45uy; 0uy; 50uy; 0uy; 52uy; 0uy;|]

            let deserialized = NodaTypeSerializerDelegate.deserialize input typeof<LocalDate>

            deserialized.GetType()
            |> should equal typeof<LocalDate>

            let localDate = deserialized :?> LocalDate

            localDate.Year   |> should equal 2015
            localDate.Month |> should equal 8
            localDate.Day |> should equal 24

        [<Fact>]
        let ``deserialize OffsiteDateTime``() =
            //2015-08-24T18:06:32.6780345+02:30
            let input : byte[] =  [|50uy; 0uy; 48uy; 0uy; 49uy; 0uy; 53uy; 0uy; 45uy; 0uy; 48uy; 0uy; 56uy; 0uy; 45uy; 0uy; 50uy; 0uy; 52uy; 0uy;
                                             84uy; 0uy; 
                                             49uy; 0uy; 56uy; 0uy; 58uy; 0uy; 48uy; 0uy; 54uy; 0uy; 58uy; 0uy; 51uy; 0uy; 50uy; 0uy;
                                             46uy; 0uy;
                                             54uy; 0uy; 55uy; 0uy; 56uy; 0uy; 48uy; 0uy; 51uy; 0uy; 52uy; 0uy; 53uy; 0uy;
                                             43uy; 0uy; 48uy; 0uy; 50uy; 0uy; 58uy; 0uy; 51uy; 0uy; 48uy; 0uy;|]

            let deserialized = NodaTypeSerializerDelegate.deserialize input typeof<OffsetDateTime>

            deserialized.GetType()
            |> should equal typeof<OffsetDateTime>

            let offsetDateTime = deserialized :?> OffsetDateTime

            offsetDateTime.Year |> should equal 2015
            offsetDateTime.Month |> should equal 8
            offsetDateTime.Day |> should equal 24
            offsetDateTime.Hour |> should equal 18
            offsetDateTime.Minute |> should equal 06
            offsetDateTime.Second |> should equal 32
            offsetDateTime.Millisecond |> should equal 678
            offsetDateTime.TickOfSecond |> should equal 6780345
            offsetDateTime.Offset.ToTimeSpan().Hours |> should equal 2
            offsetDateTime.Offset.ToTimeSpan().Minutes |> should equal 30

        [<Fact>]
        let ``serialize and deserialize LocalDate``() =
            let localDate = new LocalDate(2015, 8, 23)

            let serialized = NodaTypeSerializerDelegate.serialize localDate
            let deserialized = NodaTypeSerializerDelegate.deserialize serialized typeof<LocalDate>

            let deserializedLocalDate = deserialized :?> LocalDate

            deserializedLocalDate
            |> should equal localDate

        [<Fact>]
        let ``serialize and deserialize OffsetDateTime``() =
            let offsetDateTime = new OffsetDateTime(new LocalDateTime(2015, 8, 23, 19, 21, 32), Offset.FromHoursAndMinutes(3,32))

            let serialized = NodaTypeSerializerDelegate.serialize offsetDateTime
            let deserialized = NodaTypeSerializerDelegate.deserialize serialized typeof<OffsetDateTime>

            let deserializedOffsetDateTime = deserialized :?> OffsetDateTime

            deserializedOffsetDateTime
            |> should equal offsetDateTime

        [<Fact>]
        let ``serialize and deserialize Duration``() =
            let duration = Duration.FromSeconds 783467L

            let serialized = NodaTypeSerializerDelegate.serialize duration
            let deserialized = NodaTypeSerializerDelegate.deserialize serialized typeof<Duration>

            let deserializedDuration = deserialized :?> Duration

            deserializedDuration
            |> should equal duration

    module common =

        [<Fact>]
        let ``load schedule.json from server``() =
            let json = www.linuxwochen.common.common.loadJsonFromUri "https://cfp.linuxwochen.at/en/LWW15/public/schedule.json"

            json.Length 
            |> should greaterThan 0
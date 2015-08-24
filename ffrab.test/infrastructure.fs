module infrastructure

    open Xunit
    open FsUnit.Xunit
    open NodaTime


    module NodaTypeSerializerDelegate =

        open ffrab.mobile.common.common

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
           
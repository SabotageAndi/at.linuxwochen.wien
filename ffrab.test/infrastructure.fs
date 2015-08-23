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


module Eventbus

    open Xunit
    open FsUnit.Xunit
    open ffrab.mobile.common.eventbus
    
    [<Fact>]
    let ``One Event registered, called one time``() =
        let mutable calledCount = 0
        let isCalled x = calledCount <- calledCount + 1

        Eventbus.Current.Register isCalled Message.ChangeConference 
        new Entry(Message.ChangeConference) |>  Eventbus.Current.Publish

        calledCount |> should equal 1

    [<Fact>]
    let ``One Event registered, called two times``() =
        let mutable calledCount = 0
        let isCalled x = calledCount <- calledCount + 1

        Eventbus.Current.Register isCalled Message.ChangeConference 
        new Entry(Message.ChangeConference) |>  Eventbus.Current.Publish
        new Entry(Message.ChangeConference) |>  Eventbus.Current.Publish


        calledCount |> should equal 2


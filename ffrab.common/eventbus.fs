namespace www.linuxwochen.common

module eventbus = 
    open common
    
    type Message = 
        | ChangeConference
        | SwitchPage
        | StartLongRunningAction
        | StopLongRunningAction
        | ShowEntry
        | ShowLicense
    
    type Entry(msg) =
        member this.Message : Message = msg
        
        
        

    type RegisteredMessage = 
        { Msg : Message
          Action : Entry -> unit }
    


    type Eventbus() = 
        static let currentInstance = new Eventbus()
        let mutable registeredMessages : RegisteredMessage list = []
        
        member this.Register action msg = 
            let registeredMessage = 
                { Msg = msg
                  Action = action }
            registeredMessages <- registeredMessage :: registeredMessages
        
        member this.Publish (e : Entry) = 
            registeredMessages
            |> List.filter (fun i -> i.Msg = e.Message)
            |> List.iter (fun i -> e |> i.Action)
        
        static member Current = currentInstance

    let beginLongRunningTask() =
        new Entry(Message.StartLongRunningAction) |> Eventbus.Current.Publish
        
    let endLongRunningTask() = 
        new Entry(Message.StopLongRunningAction) |> Eventbus.Current.Publish
        
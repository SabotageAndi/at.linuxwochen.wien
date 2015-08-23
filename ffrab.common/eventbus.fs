namespace ffrab.mobile.common

module eventbus = 
    open ffrab.mobile.common.common
    
    type Message = 
        | ChangeConference
        | SwitchPage of ViewModelType
    
    type RegisteredMessage = 
        { Msg : Message
          Action : Message -> unit }
    
    type Eventbus() = 
        static let currentInstance = new Eventbus()
        let mutable registeredMessages : RegisteredMessage list = []
        
        member this.Register action msg = 
            let registeredMessage = 
                { Msg = msg
                  Action = action }
            registeredMessages <- registeredMessage :: registeredMessages
        
        member this.Publish(msg : Message) = 
            registeredMessages
            |> List.filter (fun i -> i.Msg = msg)
            |> List.iter (fun i -> i.Msg |> i.Action)
        
        static member Current = currentInstance

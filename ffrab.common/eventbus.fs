namespace ffrab.mobile.common

module eventbus =

    type Message = {identifier : string}
        
    type RegisteredMessage = { msg : Message; action :  (Message -> unit) }
    
    type Eventbus() =
        
        static let currentInstance = new Eventbus()
        
        let mutable registeredMessages : RegisteredMessage list = []

        member this.Register msg action =
            let registeredMessage = { msg = msg; action = action}
            registeredMessages <- registeredMessage :: registeredMessages

        member this.Publish (msg : Message) =
            registeredMessages |>
            List.filter (fun i -> i.msg = msg) |>
            List.iter (fun i -> i.msg |> i.action)

        static member Current with get() = currentInstance
            


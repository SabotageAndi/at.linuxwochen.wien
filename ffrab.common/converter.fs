namespace www.linuxwochen.ui.converter

open Xamarin.Forms



type PreserveAttribute() =
    inherit System.Attribute()

    member val AllMembers = false with get,set
    member val Conditional = false with get,set


[<Preserve>]
type NegateBooleanConverter() =

    do 
        ignore()

    interface IValueConverter with
        member this.Convert(value, targetType, parameter, culture) =
            match value with 
            | null -> null
            | :? bool as b -> not b :> obj
            | _ -> value

        member this.ConvertBack(value, targetType, parameter, culture) =
            match value with 
            | null -> null
            | :? bool as b -> not b :> obj
            | _ -> value


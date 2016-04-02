namespace www.linuxwochen.ui.code.converter

open Xamarin.Forms

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


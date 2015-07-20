namespace ffrab.mobile.common


open Xamarin.Forms

module app =

    type App() as this =
        inherit Application()

        do
            this.MainPage <- new ffrab.mobile.common.ui.MainPage()

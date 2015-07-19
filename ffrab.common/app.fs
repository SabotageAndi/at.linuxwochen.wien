namespace ffrab.mobile.common


open Xamarin.Forms

module app =

    type App() as this =
        inherit Application()

        do
            let content = new ContentPage()
            content.Content <- new Label(Text = "Hiho", VerticalOptions = LayoutOptions.CenterAndExpand, HorizontalOptions = LayoutOptions.CenterAndExpand)
            this.MainPage <- content

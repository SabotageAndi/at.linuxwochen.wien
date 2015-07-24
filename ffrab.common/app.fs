namespace ffrab.mobile.common


open Xamarin.Forms
open ffrab.mobile.common.ui
open ffrab.mobile.common.viewmodels

module app =

    type App() as this =
        inherit Application()

        let menuViewModel = new MenuViewModel()
        let mainViewModel = new viewmodels.MainViewModel()
            
        do
            mainViewModel.ConferenceTitle <- "Conf 1"
            new MenuItemViewModel("Item 1") |> menuViewModel.addMenu
            
            let mainPage = new MainPage()
            mainPage.BindingContext <- mainViewModel

            
            let menu = new Menu()
            menu.BindingContext <- menuViewModel
            
            let masterPage = new MasterDetailPage()
            masterPage.Master <- menu
            masterPage.Detail <- new NavigationPage(mainPage)

            this.MainPage <- masterPage

namespace ffrab.mobile.common


open Xamarin.Forms
open ffrab.mobile.common.ui
open ffrab.mobile.common.viewmodels

module app =

    open FSharp.ViewModule

    type MenuItemConnection = { Name : string; ViewModel : ViewModelBase; content : unit -> ContentPage }
        

    type App() as this =
        inherit Application()

        let conferenceList = { MenuItemConnection.Name = "Conferences"; ViewModel = new ConferenceListViewModel(); content = (fun (x : unit) -> new ConferenceList() :> ContentPage) }
        let about = { MenuItemConnection.Name = "About"; ViewModel = new AboutViewModel(); content = (fun x -> new ContentPage())}
        let home = { MenuItemConnection.Name = "Home"; ViewModel = new viewmodels.MainViewModel(); content = (fun x -> new MainPage() :> ContentPage )}

        let mutable masterPage : NavigationPage option = None
        let mutable masterDetailPage : MasterDetailPage = new MasterDetailPage()

        let menuItems = 
            [
                home
                conferenceList
                about
            ]

        let navigateTo menuItem = 
            let content = menuItem.content()
            content.BindingContext <- menuItem.ViewModel
            masterDetailPage.Detail <- new NavigationPage(content)
            masterDetailPage.IsPresented <- false

        let searchMenuItemAndNavigateTo (menuItemViewModel : MenuItemViewModel) =         
            let menuItem = menuItems |> List.find (fun x -> x.Name = menuItemViewModel.Name)
            navigateTo menuItem

        let menuViewModel = new MenuViewModel(searchMenuItemAndNavigateTo)
            
        do
            new MenuItemViewModel(home.Name) |> menuViewModel.addMenu
            new MenuItemViewModel(conferenceList.Name) |> menuViewModel.addMenu
            new MenuItemViewModel(about.Name) |> menuViewModel.addMenu
            
            let menu = new Menu()
            menu.BindingContext <- menuViewModel

            masterDetailPage.Master <- menu

            navigateTo home
           
            this.MainPage <- masterDetailPage

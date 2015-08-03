namespace ffrab.mobile.common

module app =

    open FSharp.ViewModule
    open Xamarin.Forms
    open ffrab.mobile.common.common
    open ffrab.mobile.common.ui
    open ffrab.mobile.common.viewmodels
    open ffrab.mobile.common.eventbus

    type MenuItemConnection = { Name : string; Type : ViewModelType; ViewModel : ViewModelBase; Content : unit -> ContentPage }
        
    type App() as this =
        inherit Application()

        let conferenceList = { MenuItemConnection.Name = "Conferences"; Type = ViewModelType.ConferenceList; ViewModel = new ConferenceListViewModel(); Content = (fun (x : unit) -> new ConferenceList() :> ContentPage) }
        let about = { MenuItemConnection.Name = "About"; Type = ViewModelType.About; ViewModel = new AboutViewModel(); Content = (fun x -> new ContentPage())}
        let home = { MenuItemConnection.Name = "Home"; Type = ViewModelType.Main; ViewModel = new viewmodels.MainViewModel(); Content = (fun x -> new MainPage() :> ContentPage )}

        let mutable masterPage : NavigationPage option = None
        let mutable masterDetailPage : MasterDetailPage = new MasterDetailPage()

        let menuItems = 
            [
                home
                conferenceList
                about
            ]

        let navigateTo menuItem = 
            let content = menuItem.Content()
            content.BindingContext <- menuItem.ViewModel
            masterDetailPage.Detail <- new NavigationPage(content)

            match menuItem.ViewModel with
            | As (viewModelShown : IViewModelShown) ->
                viewModelShown.Init()
            | _ -> ()

            masterDetailPage.IsPresented <- false

        let searchMenuItemAndNavigateTo (viewModelType)= 
            let menuItem = menuItems |> List.find (fun x -> x.Type = viewModelType)
            navigateTo menuItem


        let menuViewModel = new MenuViewModel()

        let changeConference msg = 
            ignore()

        let addNavigation menuItemConnection (menuViewModel : MenuViewModel) =
            let viewModelType = menuItemConnection.Type
            let navigate msg = searchMenuItemAndNavigateTo viewModelType
            Message.SwitchPage(viewModelType) |> Eventbus.Current.Register navigate 
            new MenuItemViewModel(menuItemConnection.Name, menuItemConnection.Type) |> menuViewModel.addMenu
            
        do
            Message.ChangeConference |> Eventbus.Current.Register changeConference

            menuViewModel |> addNavigation home 
            menuViewModel |> addNavigation conferenceList 
            menuViewModel |> addNavigation about 

            
            let menu = new Menu()
            menu.BindingContext <- menuViewModel

            masterDetailPage.Master <- menu

            navigateTo home
           
            this.MainPage <- masterDetailPage

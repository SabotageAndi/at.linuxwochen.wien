namespace ffrab.mobile.common

module app =

    open System.Collections.Generic
    open FSharp.ViewModule
    open Xamarin.Forms
    open common
    open ffrab.mobile.common.ui
    open viewmodels
    open eventbus
    open entities
    open model

        
    type App() as this =
        inherit Application()

        let conferenceList = { MenuItemConnection.Name = "Conferences"; Type = ViewModelType.ConferenceList; ViewModel = new ConferenceListViewModel(); Content = (fun (x : unit) -> new ConferenceList() :> ContentPage) }
        let about = { MenuItemConnection.Name = "About"; Type = ViewModelType.About; ViewModel = new AboutViewModel(); Content = (fun x -> new ContentPage())}
        let home = { MenuItemConnection.Name = "Home"; Type = ViewModelType.Main; ViewModel = new viewmodels.MainViewModel(); Content = (fun x -> new MainPage() :> ContentPage )}

        let mutable masterPage : NavigationPage option = None
        let mutable masterDetailPage : MasterDetailPage = new MasterDetailPage()
        let menuViewModel = new MenuViewModel()

        let mutable menuItems  : MenuItemConnection list = []
        let mutable lastMenuItem : MenuItemConnection option = None
           
        let getNewDetail menuItem =
            let content = menuItem.Content()
            content.BindingContext <- menuItem.ViewModel
            masterDetailPage.Detail <- new NavigationPage(content)

            match menuItem.ViewModel with
            | As (viewModelShown : IViewModelShown) ->
                viewModelShown.Init()
            | _ -> ()

            menuViewModel.SetCurrentItem menuItem

        let navigateTo (menuItem : MenuItemConnection) = 
            match lastMenuItem with
            | Some lmi ->
                if menuItem.Name <> lmi.Name then getNewDetail menuItem
            | None ->
                getNewDetail menuItem
        
            lastMenuItem <- Some menuItem
            masterDetailPage.IsPresented <- false

        let searchMenuItemAndNavigateTo (viewModelType : ViewModelType)= 
            let menuItem = menuItems |> List.find (fun x -> x.Type = viewModelType)
            navigateTo menuItem

        let addToNavigationInfrastructure (menuItemConnection : MenuItemConnection) (menuViewModel : MenuViewModel) =
            let viewModelType = menuItemConnection.Type
            let navigate msg = searchMenuItemAndNavigateTo viewModelType
            Message.SwitchPage(viewModelType) |> Eventbus.Current.Register navigate 
            menuItems <- menuItemConnection :: menuItems
            menuItemConnection
            
        let dateFormat = NodaTime.Text.LocalDatePattern.CreateWithInvariantCulture("dd'.'MM")

        let getConferenceDayName (item : ConferenceDay) =
            dateFormat.Format(item.Day)

        let addConferenceDayMenuItems() =
            let conf = Conferences.getActualConference()
            match conf with
            | Some conference ->
                let confData = Conferences.getConferenceData conference
                confData.Days |>
                List.sortBy (fun i -> i.Day) |>
                List.iter (fun item -> 
                    let menuItemConnection = { MenuItemConnection.Name = getConferenceDayName(item); Type = ViewModelType.Day(item.Day); ViewModel = new AboutViewModel(); Content = (fun x -> new ContentPage()) }
                    menuViewModel |> addToNavigationInfrastructure menuItemConnection |> menuViewModel.AddMenuAfter home
                    )
            | None ->
                ignore()

        let removeActualConferenceDayMenuItems() =
            let conf = Conferences.getActualConference()
            match conf with
            | Some conference ->
                let confData = Conferences.getConferenceData conference
                confData.Days |>
                List.map getConferenceDayName |>
                List.iter menuViewModel.removeMenu
            | None ->
                ignore()
            

        let changeConference msg = 
            removeActualConferenceDayMenuItems()
            addConferenceDayMenuItems()
            navigateTo home

        do
            lastMenuItem <- None
            Message.ChangeConference |> Eventbus.Current.Register changeConference

            menuViewModel |> addToNavigationInfrastructure home |> menuViewModel.addMenu
            menuViewModel |> addToNavigationInfrastructure conferenceList |> menuViewModel.addMenu
            menuViewModel |> addToNavigationInfrastructure about |> menuViewModel.addMenu

            addConferenceDayMenuItems()

            
            let menu = new Menu()
            menu.BindingContext <- menuViewModel

            masterDetailPage.Master <- menu

            navigateTo home
           
            this.MainPage <- masterDetailPage

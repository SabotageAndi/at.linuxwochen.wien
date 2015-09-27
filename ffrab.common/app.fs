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
    open SQLite.Net.Interop
    
    type App(sqlPlatform : ISQLitePlatform) as this = 
        inherit Application()
        
        let conferenceList = 
            { MenuItemConnection.Name = "Conferences"
              Type = ViewModelType.ConferenceList
              ViewModel = new ConferenceListViewModel()
              Content = (fun (x : unit) -> new ConferenceList() :> ContentView) }
        
        let about = 
            { MenuItemConnection.Name = "About"
              Type = ViewModelType.About
              ViewModel = new AboutViewModel()
              Content = (fun x -> new ContentView()) }
        
        let home = 
            { MenuItemConnection.Name = "Home"
              Type = ViewModelType.Main
              ViewModel = new viewmodels.MainViewModel()
              Content = (fun x -> new MainPage() :> ContentView) }
        
        let sqlPlatform = sqlPlatform
        let mutable masterPage : NavigationPage option = None
        let mutable masterDetailPage : MasterDetailPage = new MasterDetailPage()
        let menuViewModel = new MenuViewModel()
        let mutable menuItems : MenuItemConnection list = []
        let mutable lastMenuItem : MenuItemConnection option = None
        let mutable lastConference : Conference option = None

        let activityIndicator : ActivityIndicator = new ActivityIndicator(Color = Color.Gray, 
                                                                          HorizontalOptions = LayoutOptions.CenterAndExpand, 
                                                                          VerticalOptions = LayoutOptions.CenterAndExpand,
                                                                          IsVisible = false)
        
        let startLongRunningAction msg =
            activityIndicator.IsRunning <- true
            activityIndicator.IsVisible <- true

        let stopLongRunningAction msg =
            activityIndicator.IsRunning <- false
            activityIndicator.IsVisible <- false

        let getNewDetail menuItem = 
            let content = menuItem.Content()
            content.BindingContext <- menuItem.ViewModel

            let stackPanel = new Grid()
            stackPanel.RowSpacing <- 0.0
            stackPanel.ColumnSpacing <- 0.0
            stackPanel.VerticalOptions <- LayoutOptions.FillAndExpand
            stackPanel.Children.Add activityIndicator
            stackPanel.Children.Add content

            let contentPage = new ContentPage()
            contentPage.Content <- stackPanel

            masterDetailPage.Detail <- new NavigationPage(contentPage)
            match menuItem.ViewModel with
            | As(viewModelShown : IViewModelShown) -> viewModelShown.Init()
            | _ -> ()
            menuViewModel.SetCurrentItem menuItem
        
        let navigateTo (menuItem : MenuItemConnection) = 
            match lastMenuItem with
            | Some lmi -> 
                if menuItem.Name <> lmi.Name then getNewDetail menuItem
            | None -> getNewDetail menuItem
            lastMenuItem <- Some menuItem
            masterDetailPage.IsPresented <- false
        
        let searchMenuItemAndNavigateTo (viewModelType : ViewModelType) = 
            let menuItem = menuItems |> List.find (fun x -> x.Type = viewModelType)
            navigateTo menuItem
        
        let addToNavigationInfrastructure (menuItemConnection : MenuItemConnection) (menuViewModel : MenuViewModel) = 
            menuItems <- menuItemConnection :: menuItems
            menuItemConnection
        
        let dateFormat = NodaTime.Text.LocalDatePattern.CreateWithInvariantCulture("dd'.'MM")
        let getConferenceDayName (item : ConferenceDay) = dateFormat.Format(item.Day)
        
        let addConferenceDayMenuItems conferenceDay =
            let menuItemConnection = 
                           { MenuItemConnection.Name = getConferenceDayName (conferenceDay)
                             Type = ViewModelType.Day(conferenceDay.Day)
                             ViewModel = new DayViewModel(conferenceDay)
                             Content = (fun x -> new DayView() :> ContentView) }
            menuViewModel
            |> addToNavigationInfrastructure menuItemConnection
            |> menuViewModel.AddMenuAfter home

        let addConferenceDayMenuItems() = 
            Conferences.getActualConferenceDays()
            |> Seq.sortByDescending (fun i -> i.Day)
            |> Seq.iter addConferenceDayMenuItems
            
        let removeActualConferenceDayMenuItems conference = 
            conference 
            |> Conferences.getConferenceDays
            |> Seq.map getConferenceDayName
            |> Seq.iter menuViewModel.RemoveMenu
          
        let changeConference msg = 
            new eventbus.Entry(Message.StartLongRunningAction) |> Eventbus.Current.Publish
            match lastConference with
            | Some conf ->
                removeActualConferenceDayMenuItems conf 
            | _ ->
                ignore()

            model.Conferences.synchronizeData()
            addConferenceDayMenuItems()
            lastConference <- model.Conferences.getActualConference()
            navigateTo home
            new eventbus.Entry(Message.StopLongRunningAction) |> Eventbus.Current.Publish
        
        let navigate (data : eventbus.Entry) =
            match data with
            | :? SwitchPageEvent as switchPageEvent ->
                searchMenuItemAndNavigateTo switchPageEvent.Typ
            | _ ->
                ignore()

        let gotoEntry msg =
            ignore()

        do 
            lastMenuItem <- None
            Message.ChangeConference |> Eventbus.Current.Register changeConference
            Message.StartLongRunningAction |> Eventbus.Current.Register startLongRunningAction
            Message.StopLongRunningAction |> Eventbus.Current.Register stopLongRunningAction
            Message.SwitchPage |> Eventbus.Current.Register navigate
            

            menuViewModel
            |> addToNavigationInfrastructure home
            |> menuViewModel.AddMenu

            menuViewModel
            |> addToNavigationInfrastructure conferenceList
            |> menuViewModel.AddMenu

            menuViewModel
            |> addToNavigationInfrastructure about
            |> menuViewModel.AddMenu

            let menu = new Menu()
            menu.BindingContext <- menuViewModel
            masterDetailPage.Master <- menu
            navigateTo home
            this.MainPage <- masterDetailPage

        override this.OnStart() =
            model.Init sqlPlatform
            model.Conferences.synchronizeData()
            addConferenceDayMenuItems()
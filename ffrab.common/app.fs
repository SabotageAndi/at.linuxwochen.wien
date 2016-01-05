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
    
    type App(sqlPlatform : ISQLitePlatform, databasePath : string) as this = 
        inherit ffrab.mobile.common.ui.AppXaml()
        
        let about = 
            { MenuItemConnection.Name = "About"
              Type = ViewModelType.About
              ViewModel =( fun _ -> new AboutViewModel() :> ViewModelBase)
              Content = (fun _ -> new ContentView()) }
        
        let home = 
            { MenuItemConnection.Name = "Home"
              Type = ViewModelType.Main
              ViewModel = ( fun _ -> new viewmodels.MainViewModel() :> ViewModelBase)
              Content = (fun _ -> new MainPage() :> ContentView) }

        let conferenceList = 
            { MenuItemConnection.Name = "Conferences"
              Type = ViewModelType.ConferenceList
              ViewModel = ( fun _ -> new ConferenceListViewModel() :> ViewModelBase)
              Content = (fun (x : unit) -> new ConferenceList() :> ContentView) }


        
        let sql = (sqlPlatform, databasePath)

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
            let onUI() =
                activityIndicator.IsRunning <- true
                activityIndicator.IsVisible <- true
            onUI |> common.runOnUIthread

        let stopLongRunningAction msg =
            let onUI() =
                activityIndicator.IsRunning <- false
                activityIndicator.IsVisible <- false
            onUI |> common.runOnUIthread

        let getNewDetail menuItem = 
            let viewModel = menuItem.ViewModel()
            let content = menuItem.Content()
            content.BindingContext <- viewModel

            let stackPanel = new Grid()
            stackPanel.RowSpacing <- 0.0
            stackPanel.ColumnSpacing <- 0.0
            stackPanel.VerticalOptions <- LayoutOptions.FillAndExpand
            stackPanel.Children.Add activityIndicator
            stackPanel.Children.Add content

            let contentPage = new ContentPage()
            contentPage.Content <- stackPanel

            let acutalConference = model.Conferences.getActualConference()
            match acutalConference with
            | Some acutalConference ->
                contentPage.Title <- acutalConference.Name
            | None ->
                contentPage.Title <- ""

            masterDetailPage.Detail <- new NavigationPage(contentPage)
            match viewModel with
            | As(viewModelShown : IViewModelShown) -> initViewModel viewModelShown
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
        
        let searchMenuItemAndNavigateTo (viewModelType : ViewModelType) = 
            let menuItem = menuItems |> List.find (fun x -> x.Type = viewModelType)
            navigateTo menuItem
        
        let addToNavigationInfrastructure (menuItemConnection : MenuItemConnection) = 
            menuItems <- menuItemConnection :: menuItems
            menuItemConnection
        
        let dateFormat = NodaTime.Text.LocalDatePattern.CreateWithInvariantCulture("dd'.'MM")
        let getConferenceDayName (item : ConferenceDay) dayCounter = sprintf "%s - Day %i" (dateFormat.Format(item.Day)) dayCounter
        
        let addConferenceDayMenuItems conferenceDay dayCounter =
            let menuItemConnection = 
                           { MenuItemConnection.Name = getConferenceDayName conferenceDay dayCounter
                             Type = ViewModelType.Day(conferenceDay.Day)
                             ViewModel = (fun _ -> new DayViewModel(conferenceDay) :> ViewModelBase)
                             Content = (fun _ -> new DayView() :> ContentView) }
            
            addToNavigationInfrastructure menuItemConnection
            |> menuViewModel.AddMenuAfter home

        let addConferenceDayMenuItems() = 
            let days = Conferences.getActualConferenceDays()
            let mutable dayCounter = days.Length
            
            days
            |> Seq.sortByDescending (fun i -> i.Day)
            |> Seq.iter (fun d -> 
                        addConferenceDayMenuItems d dayCounter
                        dayCounter <- dayCounter - 1)
            
        let removeActualConferenceDayMenuItems conference = 
            let mutable dayCounter = 1
            conference 
            |> Conferences.getConferenceDays
            |> Seq.sortBy (fun i -> i.Day)
            |> Seq.iter (fun d ->
                        let menuItemText = getConferenceDayName d dayCounter
                        menuViewModel.RemoveMenu menuItemText
                        dayCounter <- dayCounter + 1)
          
        let changeConference msg = 
            new eventbus.Entry(Message.StartLongRunningAction) |> Eventbus.Current.Publish

            async {

                match lastConference with
                | Some conf ->
                    removeActualConferenceDayMenuItems conf 
                | _ ->
                    ignore()


                model.Conferences.synchronizeData()

                let onUI() =
                    addConferenceDayMenuItems()
                    lastConference <- model.Conferences.getActualConference()
                    navigateTo home

                    match lastConference with
                    | Some conf ->
                        masterDetailPage.Detail.Title <- conf.Name
                    | _ ->
                        ignore()

                    new eventbus.Entry(Message.StopLongRunningAction) |> Eventbus.Current.Publish

                onUI |> common.runOnUIthread

            } |> Async.Start
        
        let navigate (data : eventbus.Entry) =
            match data with
            | :? SwitchPageEvent as switchPageEvent ->
                searchMenuItemAndNavigateTo switchPageEvent.Typ
            | _ ->
                ignore()

        let gotoEntry (data : eventbus.Entry) =
            match data with
            | :? EntrySelected as entrySelected ->
                let viewModel = new EntryViewModel(entrySelected.Entry)
                initViewModel viewModel
                let view = new EntryView()
                view.BindingContext <- viewModel

                masterDetailPage.Detail.Navigation.PushAsync view |> Async.AwaitTask |> ignore
            | _ ->
                ignore()

        let addEventListeners() = 
            Message.ChangeConference |> Eventbus.Current.Register changeConference
            Message.StartLongRunningAction |> Eventbus.Current.Register startLongRunningAction
            Message.StopLongRunningAction |> Eventbus.Current.Register stopLongRunningAction
            Message.SwitchPage |> Eventbus.Current.Register navigate
            Message.ShowEntry |> Eventbus.Current.Register gotoEntry

        let addMenuItems() = 
            addToNavigationInfrastructure home
            |> menuViewModel.AddMenu
            
//            addToNavigationInfrastructure conferenceList
//            |> menuViewModel.AddMenu
            
            addToNavigationInfrastructure about
            |> menuViewModel.AddMenu

        do 
            lastMenuItem <- None
            addEventListeners()

            addMenuItems()

            let menu = new Menu()
            menu.BindingContext <- menuViewModel

            masterDetailPage.Master <- menu
            masterDetailPage.Detail <- new ContentPage(Content = new LoadingView())
            
            this.MainPage <- masterDetailPage


        override this.OnStart() =
            model.Init sql
            async {
                new eventbus.Entry(Message.StartLongRunningAction) |> Eventbus.Current.Publish
                
            
                model.Conferences.synchronizeData()
                lastConference <- model.Conferences.getActualConference()

                let onUI() = 
                    addConferenceDayMenuItems()
                    
                    navigateTo home

                    new eventbus.Entry(Message.StopLongRunningAction) |> Eventbus.Current.Publish


                onUI |> common.runOnUIthread
            } |> Async.Start

            
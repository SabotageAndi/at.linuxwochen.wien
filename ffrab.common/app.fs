namespace www.linuxwochen.common

module app = 
    open System.Collections.Generic
    open ViewModule
    open Xamarin.Forms
    open common
    open viewmodels
    open eventbus
    open entities
    open model
    open SQLite
    open www.linuxwochen.ui.converter
    open www.linuxwochen.common.ui
    
    type App(databasePath : string) as this = 
        inherit AppXaml()
        
        let about = 
            { 
              Name = "Info"
              Type = ViewModelType.About
              ViewModel =( fun _ -> new AboutViewModel() :> ViewModelBase )
              Content = (fun _ -> new Info() :> ContentView) 
              HasRefresh = false
            }
        
        let home = 
            { 
              Name = "Home"
              Type = ViewModelType.Main
              ViewModel = ( fun _ -> new viewmodels.MainViewModel() :> ViewModelBase)
              Content = (fun _ -> new MainPage() :> ContentView) 
              HasRefresh = true
            }

        let conferenceList = 
            { 
              Name = "Conferences"
              Type = ViewModelType.ConferenceList
              ViewModel = ( fun _ -> new ConferenceListViewModel() :> ViewModelBase)
              Content = (fun (x : unit) -> new ConferenceList() :> ContentView) 
              HasRefresh = false
            }
        
        let sql = databasePath

        let mutable masterDetailPage : MasterDetailPage = new MasterDetailPage()
        let menuViewModel = new MenuViewModel()
        let mutable menuItems : MenuItemConnection list = []
        let mutable lastMenuItem : MenuItemConnection option = None
        let mutable lastConference : Conference option = None

        let setLastConference conference =
            lastConference <- conference

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

            if (menuItem.HasRefresh) then
                let refreshButton = new ToolbarItem()
                refreshButton.Text <- "Refresh"
                refreshButton.Icon <- FileImageSource.FromFile("ic_cached_black_36dp.png") :?> FileImageSource
                refreshButton.Order <- ToolbarItemOrder.Primary
                refreshButton.Priority <- 0

                match viewModel with
                | As(refresh : IRefresh) -> refreshButton.Command <- refresh.RefreshCommand
                | _ -> ignore() 

                contentPage.ToolbarItems.Add(refreshButton)

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
        
        let getConferenceDayName (item : ConferenceDay) dayCounter = 
            sprintf "%s - Tag %i" (common.Formatting.shortdateFormat.Format(item.Day)) dayCounter
        
        let createDayMenu conferenceDay dayCounter=
            { 
                MenuItemConnection.Name = getConferenceDayName conferenceDay dayCounter;
                Type = ViewModelType.Day(conferenceDay.Day);
                ViewModel = (fun _ -> new DayViewModel(conferenceDay) :> ViewModelBase);
                Content = (fun _ -> new DayView() :> ContentView) 
                HasRefresh = false
            }

        let addConferenceDayMenuItems conferenceDay dayCounter =
            createDayMenu conferenceDay dayCounter
            |> addToNavigationInfrastructure 
            |> menuViewModel.AddMenuAfter home

        let addConferenceDayMenuItems conference = 
            conference 
            |> queries.getConferenceDays
            |> List.sortBy (fun i -> i.Day)
            |> List.indexed
            |> Seq.sortByDescending (fun (_,d) -> d.Day)
            |> Seq.iter (fun (i,d) -> addConferenceDayMenuItems d (i+1))
            
        let removeActualConferenceDayMenuItems conference = 
            conference 
            |> queries.getConferenceDays
            |> Seq.sortBy (fun i -> i.Day)
            |> Seq.indexed
            |> Seq.iter (fun (i,d) ->
                        let menuItemText = getConferenceDayName d (i+1)
                        menuViewModel.RemoveMenu menuItemText)
          
        let endConferenceLoad() =
            match lastConference with
            | Some conf ->
                addConferenceDayMenuItems conf
            | _ ->
                ignore()

            navigateTo home

            endLongRunningTask()

        let changeConference msg = 
            beginLongRunningTask()

            async {
                match lastConference with
                | Some conf ->
                    removeActualConferenceDayMenuItems conf 
                | _ ->
                    ignore()

                model.Conferences.getActualConference()
                |> model.Conferences.synchronizeData
                |> setLastConference 

                endConferenceLoad |> common.runOnUIthread

            } |> Async.Start
        
        let navigate (data : eventbus.Entry) =
            match data with
            | As(switchPageEvent : SwitchPageEvent) -> searchMenuItemAndNavigateTo switchPageEvent.Typ
            | _ -> ignore()

        let gotoEntry (data : eventbus.Entry) =
            match data with
            | As(entrySelected : EntrySelected) ->
                let viewModel = new EntryViewModel(entrySelected.Entry)
                initViewModel viewModel
                let view = new EntryView(BindingContext = viewModel)
                
                masterDetailPage.Detail.Navigation.PushAsync view |> Async.AwaitTask |> ignore
            | _ -> ignore()

        let showLicense e =
            let view = new License()
            masterDetailPage.Detail.Navigation.PushAsync view |> Async.AwaitTask |> ignore
            

        let addEventListeners() = 
            Message.ChangeConference |> Eventbus.Current.Register changeConference
            Message.StartLongRunningAction |> Eventbus.Current.Register startLongRunningAction
            Message.StopLongRunningAction |> Eventbus.Current.Register stopLongRunningAction
            Message.SwitchPage |> Eventbus.Current.Register navigate
            Message.ShowEntry |> Eventbus.Current.Register gotoEntry
            Message.ShowLicense |> Eventbus.Current.Register showLicense

        let addMenuItems() = 
            addToNavigationInfrastructure home
            |> menuViewModel.AddMenu
            
            addToNavigationInfrastructure about
            |> menuViewModel.AddMenu

        do 
            this.Resources.Add("NegateBooleanConverter", new NegateBooleanConverter())

            lastMenuItem <- None

            addEventListeners()
            addMenuItems()

            masterDetailPage.Master <- new Menu(BindingContext = menuViewModel)
            masterDetailPage.Detail <- new ContentPage(Content = new LoadingView())
            
            this.MainPage <- masterDetailPage


        override this.OnStart() =
            model.Init sql

            async {

                model.SyncWithUi()
                |> setLastConference

                endConferenceLoad |> common.runOnUIthread
            } |> Async.Start

            
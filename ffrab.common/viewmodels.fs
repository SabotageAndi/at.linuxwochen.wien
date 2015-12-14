namespace ffrab.mobile.common

open System.Collections.Generic
open System.Collections.ObjectModel
open FSharp.ViewModule
open System.Linq
open Xamarin.Forms
open NodaTime
open System

module viewmodels = 
    open ffrab.mobile.common.common
    open ffrab.mobile.common.model
    open ffrab.mobile.common.eventbus
    
    type IViewModelShown = 
        abstract Init : unit -> unit
    
    type AboutViewModel() = 
        inherit ViewModelBase()
    
    type MenuItemConnection = 
        { Name : string
          Type : ViewModelType
          ViewModel : unit -> ViewModelBase
          Content : unit -> ContentView }
    
    type MenuItemViewModel(menuItemConnection) = 
        inherit ViewModelBase()
        let menuItemConnection = menuItemConnection
        member this.Name = menuItemConnection.Name
        member this.Type = menuItemConnection.Type
    
    type SwitchPageEvent(msg, typ) =
        inherit eventbus.Entry(msg)

        member this.Typ = typ

    type MenuViewModel() as self = 
        inherit ViewModelBase()
        let items = self.Factory.Backing(<@ self.Items @>, new ObservableCollection<MenuItemViewModel>())
        let selectedItem = self.Factory.Backing(<@ self.SelectedItem @>, items.Value.FirstOrDefault())
        
        let removeMenuEntry entry =
            items.Value.Remove(entry) |> ignore

        member this.AddMenu item = 
            let itemViewModel = new MenuItemViewModel(item)
            items.Value.Add itemViewModel
        
        member this.AddMenuAfter after item = 
            let index = 
                items.Value
                |> List.ofSeq
                |> List.findIndex (fun i -> i.Type = after.Type)
            items.Value.Insert(index + 1, new MenuItemViewModel(item))
        
        member this.RemoveMenu name = 
            items.Value
            |> List.ofSeq
            |> List.filter (fun i -> i.Name = name)
            |> List.iter removeMenuEntry
        
        member this.Items 
            with get() = items.Value
        
        member this.SelectedItem 
            with get () = selectedItem.Value
            and set (v) = 
                selectedItem.Value <- v
                new SwitchPageEvent(Message.SwitchPage, selectedItem.Value.Type) |> Eventbus.Current.Publish
        
        member this.SetCurrentItem item = 
            let mivm = 
                items.Value
                |> List.ofSeq
                |> List.tryFind (fun i -> i.Type = item.Type && i.Name = item.Name)
            match mivm with
            | Some x -> selectedItem.Value <- x
            | _ -> ignore()
    
    type ConferenceListViewModel() as self = 
        inherit ViewModelBase()
        let items = self.Factory.Backing(<@ self.Items @>, new ObservableCollection<Conference>())
        let selectedItem = self.Factory.Backing(<@ self.SelectedItem @>, None)
        
        interface IViewModelShown with
            member this.Init() = 
                items.Value <- new ObservableCollection<Conference>(Conferences.getAllConferences())
        
        member this.Items 
            with get() = items.Value
        
        member this.SelectedItem 
            with get () = 
                match selectedItem.Value with
                | Some v -> v
                | _ -> null
            and set (v) = 
                match v with
                | null -> selectedItem.Value <- None
                | _ -> 
                    selectedItem.Value <- Some v
                    model.Conferences.setActualConference selectedItem.Value.Value
                    new eventbus.Entry(Message.ChangeConference) |>  Eventbus.Current.Publish
    
    type MainViewModel() as self = 
        inherit ViewModelBase()
        let mutable conference : Conference option = None

        let nextFavoriteEvents = self.Factory.Backing(<@ self.NextFavoriteEvents @>, new ObservableCollection<entities.Entry>())
        
        interface IViewModelShown with
            member this.Init() = 
                conference <- model.Conferences.getActualConference()
                nextFavoriteEvents.Value.Clear()

                model.Conferences.Entry.getTopFavorites 5
                |> List.iter nextFavoriteEvents.Value.Add

                self.RaisePropertyChanged(<@ self.ConferenceTitle @>)
        
        member this.ConferenceTitle = 
            match conference with
            | Some conf -> conf.Name
            | _ -> ""

        member this.NextFavoriteEvents = nextFavoriteEvents.Value

    [<AllowNullLiteralAttribute>]
    type DayItemViewModel(entry : entities.Entry) =

        let entry = entry

        member this.Entry = entry

        member this.Title = entry.Title

    [<AllowNullLiteralAttribute>]
    type GroupDayItemViewModel(startTime : OffsetDateTime, itemViewModels : DayItemViewModel list) =
        inherit ObservableCollection<DayItemViewModel>(itemViewModels)

        let startTime = startTime

        member this.StartTime 
            with get() =
                common.Formatting.durationOffsetFormat.Format startTime

        

    type EntrySelected(msg, entry : entities.Entry) =
        inherit eventbus.Entry(msg)

        member this.Entry = entry

    type DayViewModel(conferenceDay) as self =
        inherit ViewModelBase()
        let items = self.Factory.Backing(<@ self.Items @>, new ObservableCollection<GroupDayItemViewModel>())
        let selectedItem = self.Factory.Backing(<@ self.SelectedItem @>, None)
        
        let conferenceDay = conferenceDay

        interface IViewModelShown with
            member this.Init() =
                let viewModels = conferenceDay
                                 |> model.Conferences.getEntriesForDay
                                 |> List.groupBy (fun e -> e.Start)
                                 |> List.map (fun (key, value) -> (key, value |> List.map (fun i -> new DayItemViewModel(i))))
                                 |> List.map (fun (key, value) -> new GroupDayItemViewModel(key, value))
                                 
                items.Value <- new ObservableCollection<GroupDayItemViewModel>(viewModels)

        member this.Items = items.Value

        member this.SelectedItem 
            with get () : DayItemViewModel = 
                match selectedItem.Value with
                | Some v -> v
                | _ -> null
            and set (v) = 
                match v with
                | null -> selectedItem.Value <- None
                | _ -> 
                    selectedItem.Value <- Some v
                    new EntrySelected(Message.ShowEntry, v.Entry) |> Eventbus.Current.Publish
                    

    type EntryViewModel(entry : entities.Entry) as self =
        inherit ViewModelBase()

        let onFavorite() =
            model.Conferences.Entry.toggleEntryFavorite entry

        let favoriteCommand = self.Factory.CommandSync onFavorite
        let entry = entry
        let mutable room : entities.Room option = None

        interface IViewModelShown with
            member this.Init() =
                room <- model.Conferences.getRoom entry.RoomGuid

        member this.Title 
            with get() =
                entry.Title

        member this.Time
            with get() =
                sprintf "%s - %s min" this.BeginTime this.Duration

        member this.BeginTime 
            with get() =
                common.Formatting.durationOffsetFormat.Format entry.Start
            
        member this.Duration
            with get() = 
                common.Formatting.durationFormat.Format entry.Duration

        member this.Room
            with get() =
                match room with
                | Some r ->
                    r.Name
                | None ->
                    ""

        member this.Track
            with get() =
                entry.Track

        member this.Content
            with get() =
                entry.Abstract

        member this.Speaker
            with get() =
                let names = entry.Speaker |> List.map (fun s -> s.Name) 
                String.Join(", ", names)

        member this.FavoriteCommand
            with get() =
                favoriteCommand
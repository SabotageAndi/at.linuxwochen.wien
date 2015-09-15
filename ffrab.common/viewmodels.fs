namespace ffrab.mobile.common

open System.Collections.Generic
open System.Collections.ObjectModel
open FSharp.ViewModule
open System.Linq
open Xamarin.Forms

module viewmodels = 
    open ffrab.mobile.common.common
    open ffrab.mobile.common.model
    open ffrab.mobile.common.eventbus
    
    type IViewModelShown = 
        abstract Init : unit -> unit
    
    type AboutViewModel() as self = 
        inherit ViewModelBase()
    
    type MenuItemConnection = 
        { Name : string
          Type : ViewModelType
          ViewModel : ViewModelBase
          Content : unit -> ContentView }
    
    type MenuItemViewModel(menuItemConnection) as self = 
        inherit ViewModelBase()
        let menuItemConnection = menuItemConnection
        member this.Name = menuItemConnection.Name
        member this.Type = menuItemConnection.Type
    
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
            items.Value.ToArray()
            |> List.ofSeq
            |> List.filter (fun i -> i.Name = name)
            |> List.iter removeMenuEntry
        
        member this.Items = items.Value
        
        member this.SelectedItem 
            with get () = selectedItem.Value
            and set (v) = 
                selectedItem.Value <- v
                Message.SwitchPage(selectedItem.Value.Type) |> Eventbus.Current.Publish
        
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
            member this.Init() = items.Value <- new ObservableCollection<Conference>(Conferences.getAllConferences())
        
        member this.Items = items.Value
        
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
                    Eventbus.Current.Publish Message.ChangeConference
    
    type MainViewModel() as self = 
        inherit ViewModelBase()
        let mutable conference : Conference option = None
        
        interface IViewModelShown with
            member this.Init() = 
                conference <- model.Conferences.getActualConference()
                self.RaisePropertyChanged(<@ self.ConferenceTitle @>)
        
        member this.ConferenceTitle = 
            match conference with
            | Some conf -> conf.Name
            | _ -> ""

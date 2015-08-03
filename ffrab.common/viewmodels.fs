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
        abstract member Init : unit -> unit

    type AboutViewModel() as self =
        inherit ViewModelBase()

    type MenuItemConnection = { Name : string; Type : ViewModelType; ViewModel : ViewModelBase; Content : unit -> ContentPage }
    
    
    type MenuItemViewModel(menuItemConnection) as self = 
        inherit ViewModelBase()

        let menuItemConnection = menuItemConnection
       
        member this.Name with get() = menuItemConnection.Name
        member this.Type with get() = menuItemConnection.Type


    type MenuViewModel() as self =
        inherit ViewModelBase()

        let items = self.Factory.Backing(<@ self.Items @>, new ObservableCollection<MenuItemViewModel>() )
        let selectedItem = self.Factory.Backing(<@ self.SelectedItem @>, items.Value.FirstOrDefault())

        member this.addMenu item =
            let itemViewModel = new MenuItemViewModel(item)
            items.Value.Add itemViewModel

        member this.AddMenuAfter after item =
            let index = items.Value |>
                        List.ofSeq |>
                        List.findIndex (fun i -> i.Type = after.Type)
            items.Value.Insert(index, new MenuItemViewModel(item))

        member this.removeMenu name =
            items.Value.ToArray() |> 
            List.ofSeq |>
            List.filter (fun i -> i.Name = name) |>
            List.iter (fun i -> items.Value.Remove(i) |> ignore)

        member this.Items with get() = items.Value
        member this.SelectedItem 
            with get() = selectedItem.Value 
            and set(v) = 
                selectedItem.Value <- v
                Message.SwitchPage(selectedItem.Value.Type) |> Eventbus.Current.Publish 

    type ConferenceListViewModel() as self =
        inherit ViewModelBase()
       

        let items = self.Factory.Backing(<@ self.Items @>, new ObservableCollection<Conference>())
        let selectedItem = self.Factory.Backing(<@ self.SelectedItem @>, None)

      
        interface IViewModelShown with
            member this.Init() = 
                 items.Value <- new ObservableCollection<Conference>(Conferences.getAllConferences())

        member this.Items with get() = items.Value
        member this.SelectedItem 
            with get() = 
                match selectedItem.Value with
                | Some v ->
                    v
                | _ ->
                    null
            
            and set(v) = 
                match v with
                | null ->
                    selectedItem.Value <- None
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
            
            
        member this.ConferenceTitle 
            with get() =
                match conference with
                | Some conf ->
                    conf.Name
                | _ ->
                    ""
       

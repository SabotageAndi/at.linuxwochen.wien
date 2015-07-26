namespace ffrab.mobile.common

open System.Collections.Generic
open System.Collections.ObjectModel
open FSharp.ViewModule
open System.Linq

module viewmodels =
 
    open ffrab.mobile.common.model

    type AboutViewModel() as self =
        inherit ViewModelBase()
    
    type MenuItemViewModel(name : string) as self = 
        inherit ViewModelBase()

        let name = self.Factory.Backing(<@ self.Name @>, name)

        member this.Name with get() = name.Value


    type MenuViewModel(navigateTo : (MenuItemViewModel -> unit)) as self =
        inherit ViewModelBase()

        let items = self.Factory.Backing(<@ self.Items @>, new ObservableCollection<MenuItemViewModel>() )
        let selectedItem = self.Factory.Backing(<@ self.SelectedItem @>, items.Value.FirstOrDefault())

        member this.addMenu item =
            items.Value.Add item

        member this.Items with get() = items.Value
        member this.SelectedItem 
            with get() = selectedItem.Value 
            and set(v) = 
                selectedItem.Value <- v
                navigateTo selectedItem.Value

    type ConferenceListViewModel() as self =
        inherit ViewModelBase()

        let items = self.Factory.Backing(<@ self.Items @>, new ObservableCollection<Conference>())

        do
            items.Value <- new ObservableCollection<Conference>(Conferences.getAllConferences())


        member this.Items with get() = items.Value

      
    type MainViewModel() as self =
        inherit ViewModelBase()

        let conferenceTitle = self.Factory.Backing(<@ self.ConferenceTitle @>, "")

        member this.ConferenceTitle with get() = conferenceTitle.Value and set(v) = conferenceTitle.Value <- v
       


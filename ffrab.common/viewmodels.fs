namespace ffrab.mobile.common

open System.Collections.Generic
open System.Collections.ObjectModel
open FSharp.ViewModule

module viewmodels =
    
    type MenuItemViewModel(name) as self = 
        inherit ViewModelBase()

        let name = self.Factory.Backing(<@ self.Name @>, name)

        member this.Name with get() = name.Value


    type MenuViewModel() as self =
        inherit ViewModelBase()

        let items = self.Factory.Backing(<@ self.Items @>, new ObservableCollection<MenuItemViewModel>() )

        member this.addMenu item =
            items.Value.Add item

        member this.Items with get() = items.Value


    type MainViewModel() as self =
        inherit ViewModelBase()

        let conferenceTitle = self.Factory.Backing(<@ self.ConferenceTitle @>, "")

        member this.ConferenceTitle with get() = conferenceTitle.Value and set(v) = conferenceTitle.Value <- v
       


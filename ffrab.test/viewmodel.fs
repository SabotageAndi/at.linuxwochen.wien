namespace ffrab.mobile.test

open Xunit
open FsUnit.Xunit

open System.Linq

module viewmodel =

    module menu = 

        open ffrab.mobile.common.viewmodels
        open ffrab.mobile.common.common
        open ffrab.mobile.common.eventbus

        [<Fact>]
        let ``add menu item``() =

            let menuviewmodel = new MenuViewModel()
            let menuitem = new ffrab.mobile.common.viewmodels.MenuItemViewModel("test", ViewModelType.Main)
            menuviewmodel.addMenu menuitem
          
            menuviewmodel.Items.Count |> should equal 1
            
        [<Fact>]
        let ``new menu item has correct name``() =
            let menuitem = new ffrab.mobile.common.viewmodels.MenuItemViewModel("name", ViewModelType.Main)
            menuitem.Name |> should equal "name"
           

        [<Fact>]
        let ``handle menu item click``() =
            let menuitem = new ffrab.mobile.common.viewmodels.MenuItemViewModel("test", ViewModelType.Main)
           
            let mutable itemClickIsHandled = false

            let nav x = itemClickIsHandled <- true

            Message.SwitchPage(ViewModelType.Main) |> Eventbus.Current.Register nav 
                
            let menuviewmodel = new ffrab.mobile.common.viewmodels.MenuViewModel()
            menuviewmodel.addMenu menuitem

            menuviewmodel.SelectedItem <- menuitem

            itemClickIsHandled |> should equal true
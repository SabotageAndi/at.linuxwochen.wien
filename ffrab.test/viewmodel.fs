namespace ffrab.mobile.test

open Xunit
open FsUnit.Xunit

open System.Linq

module viewmodel =

    module menu = 

        [<Fact>]
        let ``add menu item``() =
            let nav x = ignore()

            let menuviewmodel = new ffrab.mobile.common.viewmodels.MenuViewModel(nav)
            let menuitem = new ffrab.mobile.common.viewmodels.MenuItemViewModel("test")
            menuviewmodel.addMenu menuitem
          
            menuviewmodel.Items.Count |> should equal 1
            
        [<Fact>]
        let ``new menu item has correct name``() =
            let menuitem = new ffrab.mobile.common.viewmodels.MenuItemViewModel("name")
            menuitem.Name |> should equal "name"
           

        [<Fact>]
        let ``handle menu item click``() =
            let menuitem = new ffrab.mobile.common.viewmodels.MenuItemViewModel("test")
           
            let mutable itemClickIsHandled = false

            let nav x = itemClickIsHandled <- true
                
            let menuviewmodel = new ffrab.mobile.common.viewmodels.MenuViewModel(nav)
            menuviewmodel.addMenu menuitem

            menuviewmodel.SelectedItem <- menuitem

            itemClickIsHandled |> should equal true
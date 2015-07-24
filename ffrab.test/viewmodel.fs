namespace ffrab.mobile.test

open Xunit
open FsUnit.Xunit

open System.Linq

module viewmodel =

    module menu = 

        [<Fact>]
        let ``add menu item``() =
            let menuviewmodel = new ffrab.mobile.common.viewmodels.MenuViewModel()
            let menuitem = new ffrab.mobile.common.viewmodels.MenuItemViewModel("test")
            menuviewmodel.addMenu menuitem
          
            menuviewmodel.Items.Count |> should equal 1
            
        [<Fact>]
        let ``new menu item has correct name``() =
            let menuitem = new ffrab.mobile.common.viewmodels.MenuItemViewModel("name")
            menuitem.Name |> should equal "name"
           

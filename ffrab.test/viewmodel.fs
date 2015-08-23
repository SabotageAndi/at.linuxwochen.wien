namespace ffrab.mobile.test

open Xunit
open FsUnit.Xunit

open System.Linq

module viewmodel =

    module menu = 

        open ffrab.mobile.common.viewmodels
        open ffrab.mobile.common.common
        open ffrab.mobile.common.eventbus

         let conn menuviewmodel = { MenuItemConnection.Name = "name"; Type = ViewModelType.Main; ViewModel = menuviewmodel; Content = (fun x -> null) }

        [<Fact>]
        let ``add menu item``() =
            
            let menuviewmodel = new MenuViewModel()
           
            conn(menuviewmodel) |> menuviewmodel.AddMenu 
          
            menuviewmodel.Items.Count |> should equal 1
            
        [<Fact>]
        let ``new menu item has correct name``() =
            let menuItemConnection = conn(new MainViewModel())
            let menuitem = new ffrab.mobile.common.viewmodels.MenuItemViewModel(menuItemConnection)
            menuitem.Name |> should equal "name"
           

        [<Fact>]
        let ``handle menu item click``() =
            
           
            let mutable itemClickIsHandled = false

            let nav x = itemClickIsHandled <- true

            Message.SwitchPage(ViewModelType.Main) |> Eventbus.Current.Register nav 
                
            let menuviewmodel = new ffrab.mobile.common.viewmodels.MenuViewModel()
            conn(menuviewmodel) |> menuviewmodel.AddMenu 
            
            let menuItem = menuviewmodel.Items.First()
           
            menuviewmodel.SelectedItem <- menuItem

            itemClickIsHandled |> should equal true
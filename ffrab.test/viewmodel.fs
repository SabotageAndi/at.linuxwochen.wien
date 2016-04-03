module Viewmodel

    open Xunit
    open FsUnit.Xunit
    open System.Linq

    module menu = 

        open www.linuxwochen.common.viewmodels
        open www.linuxwochen.common.common
        open www.linuxwochen.common.eventbus

        let conn menuviewmodel = { MenuItemConnection.Name = "name"; Type = ViewModelType.Main; ViewModel = (fun _ -> menuviewmodel); Content = (fun _ -> null) }

        [<Fact>]
        let ``add menu item``() =
            
            let menuviewmodel = new MenuViewModel()
           
            conn(menuviewmodel) |> menuviewmodel.AddMenu 
          
            menuviewmodel.Items.Count |> should equal 1
            
        [<Fact>]
        let ``new menu item has correct name``() =
            let menuItemConnection = conn(new MainViewModel())
            let menuitem = new MenuItemViewModel(menuItemConnection)
            menuitem.Name |> should equal "name"
           

        [<Fact>]
        let ``handle menu item click``() =
            let mutable itemClickIsHandled = false

            let nav x = itemClickIsHandled <- true

            Message.SwitchPage |> Eventbus.Current.Register nav 
                
            let menuviewmodel = new MenuViewModel()
            conn(menuviewmodel) |> menuviewmodel.AddMenu 
            
            let menuItem = menuviewmodel.Items.First()
           
            menuviewmodel.SelectedItem <- menuItem

            itemClickIsHandled |> should equal true
namespace ffrab.mobile.apple

open System
open UIKit
open Foundation
open Xamarin.Forms.Platform.iOS
open Xamarin.Forms

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit FormsApplicationDelegate ()

    override this.FinishedLaunching (app, options) =
        Forms.Init()

        let window = new UIWindow (UIScreen.MainScreen.Bounds)
        
        let databasePath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "ffrab.mobile.db")
        this.LoadApplication(new ffrab.mobile.common.app.App(new SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS(), databasePath))
           
        base.FinishedLaunching (app, options)

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main (args, null, "AppDelegate")
        0


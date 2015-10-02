namespace ffrab.mobile.droid

open System

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Views
open Android.Widget
open Xamarin.Forms
open Xamarin.Forms.Platform.Android

[<Activity (Label = "ffrab.droid", MainLauncher = true)>]
type MainActivity () =
    inherit FormsApplicationActivity ()

    let mutable count:int = 1

    override this.OnCreate (bundle) =

        base.OnCreate (bundle)

        Forms.Init (this, bundle)

        let sqlPlatform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid()
        let app = new ffrab.mobile.common.app.App(sqlPlatform)

        this.LoadApplication(app)



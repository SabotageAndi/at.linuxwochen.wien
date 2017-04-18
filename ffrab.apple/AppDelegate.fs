namespace www.linuxwochen.apple

open System
open UIKit
open Foundation
open Xamarin.Forms.Platform.iOS
open Xamarin.Forms
open System.Net
open System.Threading
open System.IO
open www.linuxwochen.common


type TestHookListener() =
    
    let _httpListener = new HttpListener()
    let mutable _running = false
    let mutable _listenThread : Thread = null

    let callback asyncResult =
        let context = _httpListener.EndGetContext(asyncResult)

        let streamReader = new StreamReader(context.Request.InputStream)
        let requestData = streamReader.ReadToEnd()
        let responseData = testhooks.Execute.Execute requestData

        let streamWriter = new StreamWriter(context.Response.OutputStream)
        streamWriter.Write(responseData)

        context.Response.StatusCode <- 200
        context.Response.StatusDescription <- "OK"
        context.Response.Close()
        

    let listening() =
        let result = _httpListener.BeginGetContext(new AsyncCallback(callback), _httpListener)
        result.AsyncWaitHandle.WaitOne() |> ignore

    let listen() =
        while _running do listening()
        

    member this.Start() =
        _httpListener.Prefixes.Add("http://*:7103/")
        _httpListener.AuthenticationSchemes <- AuthenticationSchemes.Anonymous
        _listenThread <- new Thread(new ThreadStart(listen))


        _running <- true

        _httpListener.Start()
        _listenThread.Start()

        ignore()

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit FormsApplicationDelegate ()

#if DEBUG
    let _testHookListener = new TestHookListener()
#endif

    override this.FinishedLaunching (app, options) =
        Forms.Init()

        let window = new UIWindow (UIScreen.MainScreen.Bounds)

        #if DEBUG
        //_testHookListener.Start()
        #endif
        
        let databasePath = System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), "ffrab.mobile.db")
        this.LoadApplication(new app.App(databasePath))
           
        base.FinishedLaunching (app, options)



module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main (args, null, "AppDelegate")
        0


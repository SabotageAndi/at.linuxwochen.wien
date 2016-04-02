using System;
using Android.App;
using Android.OS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace www.linuxwochen.droid
{
	[Activity(Label = "Linuxwochen Wien", Icon = "@mipmap/icon", 
	          ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation, ScreenOrientation= Android.Content.PM.ScreenOrientation.Portrait)]
	public class MainActivity : FormsApplicationActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			Forms.Init(this, savedInstanceState);

			var sqlPlatform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();

			var databasePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "linuxwochen.db");

			var app = new ffrab.mobile.common.app.App(sqlPlatform, databasePath);

			this.LoadApplication(app);
		}
	}
}



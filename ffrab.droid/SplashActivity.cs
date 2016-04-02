
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace www.linuxwochen.droid
{
    [Activity(Label = "Linuxwochen Wien", Theme="@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        protected override void OnResume()
        {
            base.OnResume();

            Task startupWork = new Task(() => {
                    Task.Delay(5000);  // Simulate a bit of startup work.
                });

            startupWork.ContinueWith(t => {
                    StartActivity(new Intent(Application.Context, typeof(MainActivity)));
                }, TaskScheduler.FromCurrentSynchronizationContext());

            startupWork.Start();
        }
    }
}
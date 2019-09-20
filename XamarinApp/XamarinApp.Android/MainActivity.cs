using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using Android.Content;

namespace XamarinApp.Droid
{
    [Activity(Label = "XamarinApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            InitServicesSubscribes();
        }

        private void InitServicesSubscribes()
        {
            MessagingCenter.Subscribe<HelloWordPage>(this, Events.Events.StartLongRunningTask, StartService);
            MessagingCenter.Subscribe<HelloWordPage>(this, Events.Events.StopLongRunningTask, StopService);
        }

        private void StartService(HelloWordPage obj)
        {
            var intent = new Intent(this, typeof(AndroidService));

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                StartForegroundService(intent);
            }
            else
            {
                StartService(intent);
            }
        }

        private void StopService(HelloWordPage obj)
        {
            var intent = new Intent(this, typeof(AndroidService));
            StopService(intent);
        }
    }
}
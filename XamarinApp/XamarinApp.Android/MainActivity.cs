using Android.App;
using Android.Content.PM;
using Android.OS;
using Xamarin.Forms;
using Android.Content;
using Newtonsoft.Json;
using XamarinApp.Services;

[assembly: UsesPermission(Android.Manifest.Permission.Flashlight)]
namespace XamarinApp.Droid
{
    [Activity(Label = "XamarinApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            InitServicesSubscribes();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void InitServicesSubscribes()
        {
            MessagingCenter.Subscribe<EventsClass>(this, Events.Events.StartService, StartTheService);
            MessagingCenter.Subscribe<EventsClass>(this, Events.Events.StopService, StopTheService);

            MessagingCenter.Subscribe<EventsClass>(this, Events.Events.StartUploadService, StartUploadService);
        }

        private void StartUploadService(EventsClass obj)
        {
            var intent = new Intent(this, typeof(UploadService));
            UploadService.UploadServiceModel = obj.UploadModel;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                StartForegroundService(intent);
            }
            else
            {
                StartService(intent);
            }
        }

        private void StopTheService(EventsClass obj)
        {
            var intent = new Intent(this, typeof(VirtualNetworkService));

            StopService(intent);
        }

        private void StartTheService(EventsClass events)
        {
            var intent = new Intent(this, typeof(VirtualNetworkService));

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                StartForegroundService(intent);
            }
            else
            {
                StartService(intent);
            }
        }

        //private void StartService(HelloWordPage obj)
        //{
        //    var intent = new Intent(this, typeof(AndroidService));

        //    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
        //    {
        //        StartForegroundService(intent);
        //    }
        //    else
        //    {
        //        StartService(intent);
        //    }
        //}

        //private void StopService(HelloWordPage obj)
        //{
        //    var intent = new Intent(this, typeof(AndroidService));
        //    StopService(intent);
        //}
    }
}
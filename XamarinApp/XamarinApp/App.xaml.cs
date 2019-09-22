using System;
using System.IO;
using System.Threading.Tasks;
using Android.Support.V4.Content;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinApp.UI;
using XamarinApp.UI.Exception;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace XamarinApp
{
    public partial class App : Application
    {
        public static System.Exception Exception { get; set; }

        public App()
        {
            try
            {
                InitializeComponent();

                PermissionCheck().Wait();

                IfDeveloper();

                MainPage = new NavigationPage(new MainPage());
            }
            catch (System.Exception e)
            {
                Exception = e;
                MainPage = new ExceptionPage();
            }
        }

        private async Task<bool> PermissionCheck()
        {
            var success = true;

            var result = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Calendar);
            if (result != PermissionStatus.Granted)
            {
                var ask = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Calendar);
                if (ask[Permission.Calendar] != PermissionStatus.Granted)
                {
                    success = false;
                }
            }

            result = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
            if (result != PermissionStatus.Granted)
            {
                var ask = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                if (ask[Permission.Storage] != PermissionStatus.Granted)
                {
                    success = false;
                }
            }

            return success;
        }

        private void IfDeveloper()
        {
            string fileName =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Configuration.UserTextFileName);

            if (File.Exists(fileName)) File.Delete(fileName);

            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Configuration.DefaultFileFolder);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var file = Path.Combine(directory, Guid.NewGuid().ToString());

            File.WriteAllText(file, "TestFile" + DateTime.UtcNow);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App.Admin;
using Android.Content;
using Android.Content.PM;
using Android.Support.V4.Content;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinApp.Droid;
using XamarinApp.UI;
using XamarinApp.UI.Exception;
using Permission = Plugin.Permissions.Abstractions.Permission;

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
                PermissionCheck2();

                //IfDeveloper();

                MainPage = new NavigationPage(new MainPage());
            }
            catch (System.Exception e)
            {
                Exception = e;
                MainPage = new ExceptionPage();
            }
        }

        private void PermissionCheck2()
        {
            var hasWriteContactsPermission = Android.App.Application.Context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage);
            if (hasWriteContactsPermission != Android.Content.PM.Permission.Granted)
            {
                //Android.App.Application.Context.ApplicationContext.
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

            #region TestFileSend
            HttpClient _client = new HttpClient
            {
                BaseAddress = Configuration.BaseUrl
            };


            byte[] fileBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var content = new MultipartFormDataContent();
            var byteArrayContent = new ByteArrayContent(fileBytes, 0, fileBytes.Length);
            content.Add(byteArrayContent, "filePieces", "file");



           var response = _client.PostAsync(
                Configuration.SendFilePieceRelativeEndpoint + "/" + Guid.NewGuid(),
                content,
                CancellationToken.None).Result;


            var respText = response.Content.ReadAsStringAsync().Result;

            #endregion


            string fileName =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    Configuration.UserTextFileName);

            //if (File.Exists(fileName)) File.Delete(fileName);

            #region CreateTestFile

            string filePath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "valami6.txt");

            File.WriteAllText(filePath, DateTime.UtcNow.ToString("F") + "_Bence");

            #endregion



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

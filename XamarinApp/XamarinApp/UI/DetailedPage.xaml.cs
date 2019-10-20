using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Android.Provider;
using Android.Support.V4.Content;
using Android.Widget;
using Java.IO;
using Plugin.FilePicker;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinApp.Droid;
using XamarinApp.Models;
using XamarinApp.Services;
using XamarinApp.Services.XamarinSpecific;
using Console = System.Console;
using File = System.IO.File;
using VisualElement = Xamarin.Forms.PlatformConfiguration.iOSSpecific.VisualElement;

namespace XamarinApp.UI
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DetailedPage : ContentPage
    {
        private DetailedPageViewModel _vm;

		public DetailedPage ()
		{
            _vm = new DetailedPageViewModel();

            BindingContext = _vm;

			InitializeComponent();

            _vm.FetchFiles();

            MessagingCenter.Subscribe<EventsClass>(this, Events.Events.Fetch, FetchRequest);
        }

        private void FetchRequest(EventsClass obj)
        {
            _vm.FetchFiles();
        }

        private async void FileItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var file = (e.SelectedItem as VirtualFile);

            var selected = await DisplayActionSheet(
                title: $"What do you want to do with this item? ({file.FileName})",
                cancel: "Cancel",
                destruction: null,
                "Open", "Delete");

            var isRefreshNeeded = false;

            switch (selected)
            {
                case "Open":
                    //isRefreshNeeded = true;
                    Toast.MakeText(Android.App.Application.Context, $"Opening {file.FileName}...", ToastLength.Short).Show();

                    MessagingCenter.Subscribe<string>(this, Events.Events.FileReceived, FileReceived);

                    _vm.File = file;

                    await _vm.OpenFile(file);
                    break;
                case "Delete":
                    var confirm = await DisplayAlert("Delete pressed", $"{file.FileName} will be deleted. Are you sure?", "Yes, delete", "Cancel");
                    if (confirm)
                    {
                        isRefreshNeeded = true;
                        await _vm.DeleteFile(file);
                        Toast.MakeText(Android.App.Application.Context, $"{file.FileName} deleted successfully!", ToastLength.Short).Show();
                    }
                    break;
                default: break;
            }

            if (isRefreshNeeded)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                await _vm.FetchFiles();
            }
        }

        private void FileReceived(string fileId)
        {
            if (fileId == _vm?.File?.FileId.ToString())
            {
                MessagingCenter.Unsubscribe<string>(this, Events.Events.FileReceived);

                if (!FileOpener.Open(_vm.File.FileId.ToString(), _vm.File.MimeType, Configuration.OpenableTempFolderName))
                {
                    DisplayAlert(
                        "No application",
                        $"You don not have an application that can open the following file type:  '{_vm.File.MimeType}'. Download a program that can handle this file type, then try again.",
                        "Ok");
                }
            }

            _vm.FetchFiles();
        }

        private void UploadNewItem(object sender, EventArgs e)
        {
            UploadNewItemTaskAsync();
        }

        private async Task UploadNewItemTaskAsync()
        {
            try
            {
                var file = await CrossFilePicker.Current.PickFile();

                var sure = await DisplayAlert(
                    "Confirm",
                    $"Are you sure you want to upload \"{file.FileName}\" to the network?",
                    "Upload",
                    "Cancel");

                if (sure)
                {
                    var mimeType = MimeTypes.GetMimeType(file.FilePath);

                    await _vm.UploadFile(file.FileName, file.DataArray, mimeType);

                    Toast.MakeText(Android.App.Application.Context, "Upload in progress. Please wait a few minutes",
                        ToastLength.Short);
                }

                //await DisplayAlert("hh", file.FileName, "Ok");
            }
            catch (System.Exception ex)
            {
                //throw;
            }
        }

        private async void Copy(object sender, EventArgs e)
        {
            //todo remove this

            var openable = Path.Combine(Configuration.RootFolder, Configuration.OpenableTempFolderName);
            var filepaces = Path.Combine(Configuration.RootFolder, Configuration.FilePiecesFolderName);

            //var toRoot =
            //    Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "tmp");

            //var toOpenable = Path.Combine(toRoot, "Openable");
            //var toFilePeaces = Path.Combine(toRoot, "FilePeaces");

            //if (!Directory.Exists(toOpenable))
            //{
            //    Directory.CreateDirectory(toOpenable);
            //}

            //if (!Directory.Exists(toFilePeaces))
            //{
            //    Directory.CreateDirectory(toFilePeaces);
            //}

            //foreach (var file in Directory.GetFiles(toOpenable))
            //{
            //    var tmpFile = Path.Combine(toOpenable, file);

            //    File.Delete(tmpFile);
            //}

            //foreach (var file in Directory.GetFiles(toFilePeaces))
            //{
            //    var tmpFile = Path.Combine(toFilePeaces, file);

            //    File.Delete(tmpFile);
            //}

            if (Directory.Exists(openable))
            {
                var client = new HttpClient
                {
                    BaseAddress = Configuration.BaseUrl
                };

                var content = new MultipartFormDataContent();

                foreach (var file in Directory.GetFiles(openable))
                {
                    var info = Path.GetFileName(file);

                    var tmpFile = Path.Combine(openable, file);
                    var bb = File.ReadAllBytes(tmpFile);

                    var fileBytes = bb;
                    var byteArrayContent = new ByteArrayContent(fileBytes, 0, fileBytes.Length);
                    content.Add(byteArrayContent, "filePieces", info);

                }

                var response = await client.PostAsync(
                    "Files/UploadOpenable",
                    content,
                    CancellationToken.None);
            }


            if (Directory.Exists(filepaces))
            {
                var client = new HttpClient
                {
                    BaseAddress = Configuration.BaseUrl
                };

                var content = new MultipartFormDataContent();

                foreach (var file in Directory.GetFiles(filepaces))
                {
                    var info = Path.GetFileName(file);

                    var tmpFile = Path.Combine(openable, file);
                    var bb = File.ReadAllBytes(tmpFile);

                    var fileBytes = bb;
                    var byteArrayContent = new ByteArrayContent(fileBytes, 0, fileBytes.Length);
                    content.Add(byteArrayContent, "filePieces", info);

                }

                var response = await client.PostAsync(
                    "Files/UploadPeaces",
                    content,
                    CancellationToken.None);
            }
        }

        private void Refresh(object sender, EventArgs e)
        {
            _vm.FetchFiles();
        }
    }
}
using System;
using System.IO;
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

                var mimeType = MimeTypes.GetMimeType(file.FilePath);

                await _vm.UploadFile(file.FileName, file.DataArray, mimeType);

                //await DisplayAlert("hh", file.FileName, "Ok");
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
    }
}
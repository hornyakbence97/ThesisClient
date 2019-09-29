using System;
using System.IO;
using Android.Provider;
using Android.Support.V4.Content;
using Android.Widget;
using Java.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinApp.Models;
using XamarinApp.Services;
using XamarinApp.Services.XamarinSpecific;
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
                    isRefreshNeeded = true;
                    Toast.MakeText(Android.App.Application.Context, $"Opening {file.FileName}...", ToastLength.Short).Show();
                    var filePath = await _vm.OpenFile(file);

                    //await Launcher.OpenAsync(new OpenFileRequest
                    //{
                    //    File = new ReadOnlyFile(file)
                    //});

                    //FileOpener.OpenFile(filePath, file.MimeType);
                    //FileOpener.OpenPDF2(filePath, file.MimeType);
                    //var f = new Uri("content://" + filePath);
                    //Device.OpenUri(f);
                    FileOpener.Open();
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
    }
}
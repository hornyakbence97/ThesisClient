using System;
using Android.App;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinApp.Services;
using ZXing.Net.Mobile.Forms;

[assembly: UsesPermission(Android.Manifest.Permission.Flashlight)]
namespace XamarinApp.UI
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NetworkEntry : ContentPage
    {
        private NetworkEntryViewModel _vm;

		public NetworkEntry ()
        {
            _vm = new NetworkEntryViewModel();
            _vm.CanContinueEvent += _vm_CanContinueEvent;

            _vm.LoadCurrentUser();

            InitializeComponent();

            BindingContext = _vm;

            _vm.IsBusy = true;

            _vm.LoginToNetworkOrCreateNew();
        }

        private void _vm_CanContinueEvent(Guid networkId)
        {
            MessagingCenter.Send(EventsClass.Instance, Events.Events.StartService);

            Navigation.PushAsync(new DetailedPage());
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void LoginNetwork(object sender, EventArgs e)
        {
            _vm.Login();
        }

        private void CreateNetwork(object sender, EventArgs e)
        {
            _vm.CreateNetwork();
        }

        private async void QrScanned(object sender, EventArgs e)
        {
            //var scanner = DependencyService.Get<IQrScanningService>();

            //var result = await scanner.ScanAsync();

            var scanPage = new ZXingScannerPage();
            NavigationPage.SetHasNavigationBar(scanPage, false);
            scanPage.OnScanResult += (result) => {
                scanPage.IsScanning = false;
                _vm.NetworkId = result.Text;
                Device.BeginInvokeOnMainThread(async () => await Navigation.PopAsync());
            };

            await Navigation.PushAsync(scanPage);
        }
    }
}
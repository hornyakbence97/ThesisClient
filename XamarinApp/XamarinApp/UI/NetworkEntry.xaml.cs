using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinApp.Services;

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
    }
}
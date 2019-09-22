using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinApp.UI
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : ContentPage
    {
        private MainPageViewModel _vm;

		public MainPage ()
        {
            _vm = new MainPageViewModel
            {
                IsBusy = true,
                ShowText = "The application is loading..."
            };

            _vm.CanContinueEvent += _vm_CanContinueEvent;
            _vm.MoreInformationNeeded += _vm_MoreInformationNeeded;

            InitializeComponent();

            BindingContext = _vm;


            //BluetoothAdapter myDevice = BluetoothAdapter.DefaultAdapter;
            //var deviceName = myDevice.Name;

            _vm.LoadUser();
        }

        private void _vm_MoreInformationNeeded()
        {
            Navigation.PushModalAsync(new NameProvider
            {
                ViewModel = _vm
            });
        }

        private void _vm_CanContinueEvent()
        {
            Navigation.PushAsync(new NetworkEntry());
        }
    }
}
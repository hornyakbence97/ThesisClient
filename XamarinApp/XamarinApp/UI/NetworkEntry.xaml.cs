using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinApp.UI
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NetworkEntry : ContentPage
    {
        private NetworkEntryViewModel _vm;

		public NetworkEntry ()
        {
            _vm = new NetworkEntryViewModel();
            _vm.LoadCurrentUser();

            InitializeComponent();

            BindingContext = _vm;
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
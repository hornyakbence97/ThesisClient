using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinApp.UI.Exception
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ExceptionPage : ContentPage
	{
        public ExceptionPage()
		{
			InitializeComponent();

            BindingContext = new {ExceptionText = App.Exception.ToString()};
        }
    }
}
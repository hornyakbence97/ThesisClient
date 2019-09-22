using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinApp.UI
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NameProvider : ContentPage
    {
        public MainPageViewModel ViewModel { get; set; }

        public NameProvider()
        {
            InitializeComponent();
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            ViewModel.AddMoreInformation(displayName.Text);
            Navigation.PopModalAsync();
        }
    }
}
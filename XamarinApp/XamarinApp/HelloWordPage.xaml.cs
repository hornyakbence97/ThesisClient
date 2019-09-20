using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinApp.Droid;

namespace XamarinApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HelloWordPage : ContentPage
	{
		public HelloWordPage()
		{
			InitializeComponent();

            InitializeSubscriptions();

 
		}

        private void InitializeSubscriptions()
        {
            MessagingCenter.Subscribe<AndroidService>(this, Events.Events.CancelledTask, CancelledTask);
            MessagingCenter.Subscribe<Model>(this, Events.Events.Message, MessageReceived);
            MessagingCenter.Subscribe<string>(this, Events.Events.WebSocketReceive, WebSocketRecieved);
        }

        private void WebSocketRecieved(string obj)
        {
            DisplayAlert("WebSocketMessage", obj, "OK", "Cancel");
        }

        private void MessageReceived(Model obj)
        {
            label.Text = obj.Date.ToString();
        }

        private void CancelledTask(AndroidService obj)
        {
            label.Text = "Finished.";
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            label.Text = string.Format($"The value is {e.NewValue:F2}");
        }

        private void Start_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send<HelloWordPage>(this, Events.Events.StartLongRunningTask);
        }

        private void Stop_Clicked(object sender, EventArgs e)
        {
            MessagingCenter.Send<HelloWordPage>(this, Events.Events.StopLongRunningTask);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinApp
{
    public class LongRunningTask
    {
        private CancellationToken cancellationToken;

        public LongRunningTask(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        public async Task StartTask()
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(1000, cancellationToken);

                Model number = new Model { Date = DateTime.Now };

                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send<Model>(number, Events.Events.Message);
                });
            }
        }
    }

    public class Model
    {
        public DateTime Date { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using XamarinApp.WebSocket;

namespace XamarinApp.Droid
{
    [Service]
    class AndroidService : Service
    {
        private CancellationTokenSource cancellationTokenSource;

        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            cancellationTokenSource = new CancellationTokenSource();

            CreateNotification();

            Task.Run(() =>
            {
                try
                {
                    //var task = new LongRunningTask(cancellationTokenSource.Token);
                    var task = new WebSocketProcesser();
                    task.StartTask().Wait();
                }

                catch (System.OperationCanceledException)
                {}
                finally
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            MessagingCenter.Send<AndroidService>(this, Events.Events.CancelledTask);
                        });
                    }
                }

            }, cancellationTokenSource.Token);

            return StartCommandResult.Sticky;
        }

        private void CreateNotification()
        {
            var notification = new Notification.Builder(this)
            .SetContentTitle("Hi")
            .SetContentText("Szoveg")
            //.SetSmallIcon(Resource.Drawable.mr_button_connecting_light)
            .Build();

            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification);
        }

        public override void OnDestroy()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                cancellationTokenSource.Cancel();
            }
        }
    }
}
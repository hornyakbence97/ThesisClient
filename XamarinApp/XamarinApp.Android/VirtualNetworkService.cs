using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using XamarinApp.Services;

namespace XamarinApp.Droid
{
    [Service]
    class VirtualNetworkService : Service
    {
        private CancellationTokenSource cancellationTokenSource;

        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10002;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            cancellationTokenSource = new CancellationTokenSource();

            CreateNotification();

            Task.Run(
                action: () => { WebsocketService.Instance.StartService(cancellationTokenSource.Token).Wait(); },
                cancellationToken: cancellationTokenSource.Token);

            return StartCommandResult.Sticky;
        }

        private void CreateNotification()
        {
            var notification = new Notification.Builder(this)
                .SetContentTitle("Hi")
                .SetContentText("Szoveg")
                .SetSmallIcon(Resource.Drawable.mr_button_connecting_light)
                .Build();

            //NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this)
            //    .SetSmallIcon(Resource.Drawable.ic_mr_button_connected_20_dark)
            //    .SetContentTitle(Configuration.NotificationContentTitle)
            //    .SetContentText(Configuration.NotificationContentText)
            //    //.SetSound(Settings.System.DefaultNotificationUri)
            //    .SetVibrate(new long[] {1000, 1000})
            //    .SetLights(Resource.Color.primary_material_light, 3000, 3000)
            //    .SetAutoCancel(true);
            //    //.SetContentIntent(pendingIntent);

            StartForeground(SERVICE_RUNNING_NOTIFICATION_ID, notification);
        }

        void CreateNotificationChannel()
        {
            //if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            //{
            //    return;
            //}

            //var channelName = Configuration.NotificationChannelName;
            //var channelDescription = Configuration.NotificationChannelDescription;
            //var channel = new NotificationChannel(
            //    "10111",
            //    channelName,
            //    NotificationImportance.Default)
            //{
            //    Description = channelDescription
            //};

            //var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            //notificationManager.CreateNotificationChannel(channel);
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
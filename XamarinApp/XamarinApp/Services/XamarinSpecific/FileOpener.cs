using System;
using System.IO;
using Android.Content;
using Android.Support.V4.Content;
using Android.Views;

namespace XamarinApp.Services.XamarinSpecific
{
    public static class FileOpener
    {
        public static bool Open(string fileName, string mimeType, string folderName = null)
        {
            //string mimeType = "text/plain";
            //string fileName = "valami6.txt";

            string filePath = Configuration.RootFolder;

            if (folderName != null)
            {
                filePath = Path.Combine(filePath, folderName);
            }

            filePath = Path.Combine(filePath, fileName);

            //var bytes = File.ReadAllBytes(filePath);

            ////Copy the private file's data to the EXTERNAL PUBLIC location
            //string externalStorageState = global::Android.OS.Environment.ExternalStorageState;
            //var externalPath = global::Android.OS.Environment.ExternalStorageDirectory.Path + "/" + global::Android.OS.Environment.DirectoryDownloads + "/" + fileName;
            //File.WriteAllBytes(externalPath, bytes);

            Java.IO.File file = new Java.IO.File(filePath);
            file.SetReadable(true);

            Intent intent = new Intent(Intent.ActionView);
            Android.Net.Uri uri = Android.Net.Uri.FromFile(file);
            Context context = Android.App.Application.Context;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
            {
                uri = FileProvider.GetUriForFile(context, context.PackageName + ".fileprovider", file);
                intent.SetDataAndType(uri, mimeType);
                intent.SetFlags(ActivityFlags.GrantReadUriPermission);
                intent.AddFlags(ActivityFlags.NoHistory);
            }
            else
            {
                intent.SetDataAndType(uri, mimeType);
                intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);
            }

            try
            {
                context.StartActivity(intent);
            }
            catch (ActivityNotFoundException)
            {
                return false;
            }

            return true;
        }
    }
}
        
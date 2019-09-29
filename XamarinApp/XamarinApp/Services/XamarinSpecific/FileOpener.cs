using System;
using System.IO;
using Android.Content;
using Android.Support.V4.Content;
using Android.Widget;
using Java.Net;
using Xamarin.Forms;
using Uri = Android.Net.Uri;

namespace XamarinApp.Services.XamarinSpecific
{
    public static class FileOpener
    {
        public static void OpenFile(string path, string mimeType)
        {
            var bytes = File.ReadAllBytes(path);

            //Copy the private file's data to the EXTERNAL PUBLIC location
            var externalPath = global::Android.OS.Environment.ExternalStorageDirectory.Path + "/" +
                               global::Android.OS.Environment.DirectoryDownloads + "/" + "valami";

            File.WriteAllBytes(externalPath, bytes);

            var file = new Java.IO.File(externalPath);
            file.SetReadable(true);

            //string application = "";
            //string extension = Path.GetExtension(filePath);

            // get mimeTye
            //switch (extension.ToLower())
            //{
            //    case ".txt":
            //        application = "text/plain";
            //        break;
            //    case ".doc":
            //    case ".docx":
            //        application = "application/msword";
            //        break;
            //    case ".pdf":
            //        application = "application/pdf";
            //        break;
            //    case ".xls":
            //    case ".xlsx":
            //        application = "application/vnd.ms-excel";
            //        break;
            //    case ".jpg":
            //    case ".jpeg":
            //    case ".png":
            //        application = "image/jpeg";
            //        break;
            //    default:
            //        application = "*/*";
            //        break;
            //}

            //Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + filePath);
            var uri = Android.Net.Uri.FromFile(file);
            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(uri, mimeType);
            intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);

            Android.App.Application.Context.StartActivity(intent);
        }

        //public static void OpenPdf(string filePath, string mimeType)
        //{
        //    filePath =
        //        Path.Combine(
        //            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        //            "valami2");

        //    mimeType = "text/plain";

        //    //var bytes = File.ReadAllBytes(filePath);

        //    ////Copy the private file's data to the EXTERNAL PUBLIC location
        //    //string externalStorageState = global::Android.OS.Environment.ExternalStorageState;
        //    //var externalPath = global::Android.OS.Environment.ExternalStorageDirectory.Path + "/" + global::Android.OS.Environment.DirectoryDownloads + "/" + "bence_" + DateTime.UtcNow.ToFileTimeUtc();
        //    //File.WriteAllBytes(externalPath, bytes);

        //    Java.IO.File file = new Java.IO.File(filePath);
        //    file.SetReadable(true);

        //    //Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + filePath);
        //    Android.Net.Uri uri = Android.Net.Uri.FromFile(file);
        //    Intent intent = new Intent(Intent.ActionView);
        //    intent.SetDataAndType(uri, mimeType);
        //    intent.set
        //    intent.SetFlags(ActivityFlags.GrantReadUriPermission);
        //    intent.SetFlags(ActivityFlags.GrantWriteUriPermission);
        //    intent.SetFlags(ActivityFlags.GrantPrefixUriPermission);
        //    intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);

        //    Forms.Context.StartActivity(intent);
        //}


        public static void OpenPDF2(string filePath, string mimeType)
        {

            mimeType = "text/plain";

            filePath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "valami2");

            var bytes = File.ReadAllBytes(filePath);

            try
            {
                var pathToPdf = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                pathToPdf = Path.Combine(pathToPdf, "valami3.txt");

                File.WriteAllBytes(pathToPdf, bytes);

                Java.IO.File myFile = new Java.IO.File(pathToPdf);

                Android.Net.Uri targetUri = global::Android.Net.Uri.FromFile(myFile);
                Intent intent = new Intent(Intent.ActionView);


                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
                {
                    targetUri = FileProvider.GetUriForFile(
                        Android.App.Application.Context,
                        Android.App.Application.Context.PackageName + ".fileprovider", myFile)
                        ;
                    intent.SetDataAndType(targetUri, mimeType);
                    intent.SetFlags(ActivityFlags.GrantReadUriPermission);
                    intent.AddFlags(ActivityFlags.NoHistory);
                }
                else
                {
                    intent.SetDataAndType(targetUri, mimeType);
                    intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);
                }

                Android.App.Application.Context.StartActivity(intent); // Adobe reader does not want to open the pdf...
            }
            catch (ActivityNotFoundException e)
            {
                //Toast.MakeText(this, "No PDF viewer installed.", ToastLength.Short).Show();
            }
            catch (System.Exception ex)
            {
                //Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
            }
        }


        public static void Open()
        {
            string mimeType = "text/plain";
            string fileName = "valami6.txt";

            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var filePath = Path.Combine(documentsPath, fileName);

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

            context.StartActivity(intent);
        }
    }
}
        
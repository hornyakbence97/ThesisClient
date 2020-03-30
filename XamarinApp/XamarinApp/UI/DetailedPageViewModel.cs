using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Xamarin.Forms;
using XamarinApp.Models;
using XamarinApp.Services;
using ZXing;
using ZXing.Mobile;
using ZXing.QrCode;
using ZXing.Rendering;
using Result = XamarinApp.Services.Helpers.Result;

namespace XamarinApp.UI
{
    class DetailedPageViewModel : BaseViewModel
    {
        private ObservableCollection<VirtualFile> _files;
        private bool _isNoAnyFile;
        private bool _isEmptyWarningEnabled;
        private VirtualFile _file;
        private ImageSource _qrCode;
        private string _qrCodeButtonText;
        private bool _isQrButtonEnabled;

        public bool IsQrButtonEnabled
        {
            get => _isQrButtonEnabled;
            set { _isQrButtonEnabled = value; OnPropertyChanged();}
        }


        public string QrCodeButtonText
        {
            get => _qrCodeButtonText;
            set { _qrCodeButtonText = value; OnPropertyChanged();}
        }

        public ImageSource QrCode
        {
            get => _qrCode;
            set { _qrCode = value; OnPropertyChanged();}
        }

        public bool IsEmptyWarningEnabled
        {
            get => _isEmptyWarningEnabled;
            set { _isEmptyWarningEnabled = value; OnPropertyChanged();}
        }

        public bool IsNoAnyFile
        {
            get => _isNoAnyFile;
            set { _isNoAnyFile = value; OnPropertyChanged();}
        }

        public VirtualFile File
        {
            get => _file;
            set { _file = value; OnPropertyChanged();}
        }

        public ObservableCollection<VirtualFile> Files
        {
            get => _files;
            set { _files = value; OnPropertyChanged();}
        }

        public DetailedPageViewModel()
        {
            IsQrButtonEnabled = true;
            QrCodeButtonText = "Show Network ID QR Code";
            QrCode = null;
            IsNoAnyFile = true;
            IsEmptyWarningEnabled = true;
            IsBusy = true;
            ShowText = "Fetching files from server...";
        }

        public async Task FetchFiles()
        {
            ShowLoading("Fetching files...");

            Files = new ObservableCollection<VirtualFile>((await VirtualFileService.Instance.FetchFileListFromServer()).OrderByDescending(x => x.Created));

            if (!Files.Any())
            {
                IsNoAnyFile = true;
            }

            IsBusy = false;

            IsEmptyWarningEnabled = true;
        }

        public async  Task DeleteFile(VirtualFile file)
        {
            ShowLoading("Delete in progress, please wait...");

            await VirtualFileService.Instance.DeleteFile(file.FileId);

            IsBusy = false;
            IsEmptyWarningEnabled = true;
        }

        public async Task OpenFile(VirtualFile file)
        {
            ShowLoading("Downloading missing file pieces, please wait...");

            await VirtualFileService.Instance.OpenFile(file);
        }

        public async Task<Result> UploadFile(string fileName, byte[] fileDataArray, string mimeType)
        {
            ShowLoading("Uploading file, please wait...");
            //await Task.Delay(3000);

            var result = await VirtualFileService.Instance.UploadNewFileToServerAsync(fileName, fileDataArray, mimeType);

            //ShowLoading("Updating files, please wait...");
            //await Task.Delay(2000); //todo remoe this

            return result;
        }

        private void ShowLoading(string loadingText)
        {
            IsBusy = true;

            // hide elements
            Files = new ObservableCollection<VirtualFile>();
            IsEmptyWarningEnabled = false;

            ShowText = loadingText;
        }

        public async  Task ShowQrCode()
        {
            IsQrButtonEnabled = false;

            var dataText = UserService.Instance.GetCurrentUser().NetworkId.Value.ToString();

            var writer = new QRCodeWriter();
            var bitMatrix = writer.encode(
                dataText,
                BarcodeFormat.QR_CODE,
                600,
                600);

            var bit = new BitmapRenderer();
            var image = bit.Render(bitMatrix, BarcodeFormat.QR_CODE, dataText);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                image.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 0, stream);
                bytes = stream.ToArray();
            }

            var imageSource = ImageSource.FromStream(() => new MemoryStream(bytes, 0, bytes.Length));

            QrCode = imageSource;

            for (int i = 0; i < 10; i++)
            {
                QrCodeButtonText = $"Show Network ID QR Code ({10-i})";

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            QrCodeButtonText = "Show Network ID QR Code";

            QrCode = null;

            IsQrButtonEnabled = true;
        }

        public async Task RemoveOpenableFolder()
        {
            await VirtualFileService.Instance.EmptyOpenableFolder();
        }

        public double GetCacheSize()
        {
            return VirtualFileService.Instance.GetOpenableFolderSize();
        }
    }
}

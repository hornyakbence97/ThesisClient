using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XamarinApp.Models;
using XamarinApp.Services;

namespace XamarinApp.UI
{
    class DetailedPageViewModel : BaseViewModel
    {
        private ObservableCollection<VirtualFile> _files;
        private bool _isNoAnyFile;
        private bool _isEmptyWarningEnabled;
        private VirtualFile _file;

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
            IsNoAnyFile = true;
            IsEmptyWarningEnabled = true;
            IsBusy = true;
            ShowText = "Fetching files from server...";
        }

        public async Task FetchFiles()
        {
            IsBusy = true;

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

        public async Task UploadFile(string fileName, byte[] fileDataArray, string mimeType)
        {
            ShowLoading("Uploading file, please wait...");
            //await Task.Delay(3000);

            await VirtualFileService.Instance.UploadNewFileToServerAsync(fileName, fileDataArray, mimeType);

            ShowText = "Updating files, please wait...";
            await Task.Delay(2000); //todo remoe this
            await FetchFiles();
        }

        private void ShowLoading(string loadingText)
        {
            IsBusy = true;

            // hide elements
            Files = new ObservableCollection<VirtualFile>();
            IsEmptyWarningEnabled = false;

            ShowText = loadingText;
        }
    }
}

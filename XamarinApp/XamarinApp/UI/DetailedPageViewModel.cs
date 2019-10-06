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
            IsEmptyWarningEnabled = true;

            Files = new ObservableCollection<VirtualFile>(await VirtualFileService.Instance.FetchFileListFromServer());
            //todo remove this
            Files = new ObservableCollection<VirtualFile>
            {
                new VirtualFile
                {
                    FileName = "Fájl 1",
                    FileSize = 5000,
                    UploadedBy = "Bence",
                    Created = DateTime.UtcNow,
                    MimeType = "application/json",
                    FileId = Guid.NewGuid()
                },
                new VirtualFile
                {
                    FileName = "Fájl 2",
                    FileSize = 1024000,
                    UploadedBy = "Sanyi",
                    Created = DateTime.UtcNow,
                    MimeType = "text/plain",
                    FileId = Guid.NewGuid()
                },
                new VirtualFile
                {
                    FileName = "Fájl 2",
                    FileSize = 1024000,
                    UploadedBy = "Sanyi",
                    Created = DateTime.UtcNow,
                    MimeType = "application/json",
                    FileId = Guid.NewGuid()
                },
                new VirtualFile
                {
                    FileName = "Fájl 2",
                    FileSize = 1024000,
                    UploadedBy = "Sanyi",
                    Created = DateTime.UtcNow,
                    MimeType = "application/json",
                    FileId = Guid.NewGuid()
                },
                new VirtualFile
                {
                    FileName = "Fájl 2",
                    FileSize = 1024000,
                    UploadedBy = "Sanyi",
                    Created = DateTime.UtcNow,
                    MimeType = "application/json",
                    FileId = Guid.NewGuid()
                }
            };

            if (!Files.Any())
            {
                IsNoAnyFile = true;
            }

            IsBusy = false;
        }

        public async  Task DeleteFile(VirtualFile file)
        {
            IsBusy = true;

            //hide elements
            Files = new ObservableCollection<VirtualFile>();
            IsEmptyWarningEnabled = false;

            ShowText = "Delete in progress...";

            await VirtualFileService.Instance.DeleteFile(file.FileId);

            IsBusy = false;
            IsEmptyWarningEnabled = true;
        }

        public async Task OpenFile(VirtualFile file)
        {
            IsBusy = true;

            //hide elements
            Files = new ObservableCollection<VirtualFile>();
            IsEmptyWarningEnabled = false;

            ShowText = "Downloading missing file pieces, please wait...";

            await VirtualFileService.Instance.OpenFile(file);

            //IsBusy = false;
            //IsEmptyWarningEnabled = true;
        }
    }
}

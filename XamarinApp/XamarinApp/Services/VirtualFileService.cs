using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Support.V4.Content;
using Newtonsoft.Json;
using XamarinApp.Exception;
using XamarinApp.Models;
using XamarinApp.Models.Dto.Input;

namespace XamarinApp.Services
{
    public sealed class VirtualFileService
    {
        private static VirtualFileService instance = null;
        private static readonly object padlock = new object();

        private HttpClient _client;

        VirtualFileService()
        {
            _client = new HttpClient
            {
                BaseAddress = Configuration.BaseUrl
            };
        }

        public static VirtualFileService Instance
        {
            get
            {
                lock (padlock)
                {
                    return instance ?? (instance = new VirtualFileService());
                }
            }
        }

        public async Task<IEnumerable<VirtualFilePiece>> GetAllVirtualFilesInPhone()
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Configuration.DefaultFileFolder);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var filesInDirectory = Directory.EnumerateFiles(directory).Select(fileName =>
            {
                var path = Path.Combine(directory, fileName);

                var info = new FileInfo(path);

                return new VirtualFilePiece
                {
                    FileSize = info.Length,
                    Id = info.Name,
                    LastModified = info.LastWriteTimeUtc
                };
            });

            return filesInDirectory;
        }

        public async Task SendFilesToServer()
        {
            var files = await GetAllVirtualFilesInPhone();

            var response = await _client.PostAsync(
                Configuration.SendFileListToServerRelativeEndpoint + UserService.Instance.GetCurrentUser().Token1,
                new StringContent(JsonConvert.SerializeObject(files), Encoding.UTF8, "application/json"),
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new OperationFailedException("Failed to upload fileId list, API call unsuccessful");
            }
        }

        public async Task<List<VirtualFile>> FetchFileListFromServer()
        {
            var user = UserService.Instance.GetCurrentUser();

            var response = await _client.PostAsync(
                Configuration.GetFileListRelativeEndpoint,
                new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"),
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new OperationFailedException("Failed to fetch file list, API call unsuccessful: " + errorMessage);
            }

            var returnJson = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<VirtualFile>>(returnJson);
        }

        public async Task DeleteFile(Guid fileFileId)
        {
            await Task.Delay(3500); //TODO implement
        }

        public async Task<string> OpenFile(Guid fileFileId)
        {
            await Task.Delay(5000); //TODO implement

            string filePath =
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "valami2.txt");

            filePath = Path.Combine(Android.OS.Environment.RootDirectory.AbsolutePath, "valami3.txt");

            return filePath;
        }
    }
}

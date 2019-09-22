using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public Task<IEnumerable<VirtualFile>> GetAllVirtualFilesInPhone()
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

                return new VirtualFile
                {
                    FileSize = info.Length,
                    Id = info.Name,
                    LastModified = info.LastWriteTimeUtc
                };
            });

            return Task.FromResult(filesInDirectory);
        }

        public async Task SendFilesToServer()
        {
            var files = GetAllVirtualFilesInPhone().Result;

            var response = await _client.PostAsync(
                Configuration.SendFileListToServerRelativeEndpoint + UserService.Instance.GetCurrentUser().Token1,
                new StringContent(JsonConvert.SerializeObject(files), Encoding.UTF8, "application/json"),
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                throw new OperationFailedException("Failed to upload fileId list, API call unsuccessful");
            }
        }
    }
}

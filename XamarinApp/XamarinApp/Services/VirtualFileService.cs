using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Support.V4.Content;
using Newtonsoft.Json;
using Xamarin.Forms;
using XamarinApp.Exception;
using XamarinApp.Models;
using XamarinApp.Models.Dto.Input;
using XamarinApp.Models.Dto.Output;
using XamarinApp.Services.Helpers;

namespace XamarinApp.Services
{
    public sealed class VirtualFileService
    {
        private static VirtualFileService instance = null;
        private static readonly object padlock = new object();

        private HttpClient _client;
        private ConcurrentDictionary<string, string> _needToReceiveTheseFilePeaces = new ConcurrentDictionary<string, string>();
        private bool _isLastFilePeace = false;
        private List<(Guid Id, int OrderId)> _filePeacesToOpen;
        private object _lockObjectSaveFilePeace = new object();

        VirtualFileService()
        {
            _client = new HttpClient
            {
                BaseAddress = Configuration.BaseUrl
            };

            MessagingCenter.Subscribe<string>(this, Events.Events.WebSocketReceive, FilePeaceArrived);
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
            var directory = Path.Combine(Configuration.RootFolder, Configuration.FilePiecesFolderName);

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
            var user = UserService.Instance.GetCurrentUser();

            var dto = new DeleteFileDto
            {
                FileId = fileFileId,
                UserToken1Id = user.Token1
            };

            var response = await _client.PostAsync(
                Configuration.DeleteFileRelativeEndpoint,
                new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json"),
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new OperationFailedException("Failed to delete file, API call unsuccessful: " + errorMessage);
            }

            //var returnJson = await response.Content.ReadAsStringAsync();
        }

        public async Task OpenFile(VirtualFile file)
        {
            var directory = Path.Combine(Configuration.RootFolder, Configuration.OpenableTempFolderName);

            lock (_lockObjectSaveFilePeace)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            //write all data to this file:
            var filePath = Path.Combine(directory, file.FileId.ToString());

            if (File.Exists(filePath))
            {
                MessagingCenter.Send<string>(file.FileId.ToString(), Events.Events.FileReceived);

                return;
            }

            var filePiecesThatNeedToForThisFileToOpen = await SendFileOpeningRequestAsync(file.FileId);
            _filePeacesToOpen = filePiecesThatNeedToForThisFileToOpen.AllIds; //all the peaces

            _isLastFilePeace = false;
            foreach (var filePeace in filePiecesThatNeedToForThisFileToOpen.MissingIds) //missing peaces
            {
                _needToReceiveTheseFilePeaces.TryAdd(filePeace.Id.ToString(), file.FileId.ToString());
            }
            _isLastFilePeace = true;

            _needToReceiveTheseFilePeaces.TryAdd(string.Empty, file.FileId.ToString());
            MessagingCenter.Send<string>(string.Empty, Events.Events.WebSocketReceive);
        }

        private void FilePeaceArrived(string id)
        {
            _needToReceiveTheseFilePeaces.TryRemove(id, out string removedId);

            if (!_needToReceiveTheseFilePeaces.Any() && _isLastFilePeace && !string.IsNullOrWhiteSpace(removedId))
            {
                PrepareFilePeacesToOpen(removedId); //union all file peace

                MessagingCenter.Send<string>(removedId, Events.Events.FileReceived);
            }
        }

        private void PrepareFilePeacesToOpen(string fileId)
        {
            if (!string.IsNullOrWhiteSpace(fileId))
            {
                var filePeacesFolder = Path.Combine(Configuration.RootFolder, Configuration.FilePiecesFolderName);
                var outputFolder = Path.Combine(Configuration.RootFolder, Configuration.OpenableTempFolderName);

                var outputFilePath = Path.Combine(outputFolder, fileId);

                //todo consider this (i mean cache-ing the data is good)
                lock (_lockObjectSaveFilePeace)
                {
                    if (File.Exists(outputFilePath))
                    {
                        File.Delete(outputFilePath);
                    }
                }

                var outp = new byte[0];

                var orderedList = _filePeacesToOpen.OrderBy(x => x.OrderId);

                foreach (var filePeace in orderedList)
                {
                        var tempFilePath = Path.Combine(filePeacesFolder, filePeace.Id.ToString());

                        byte[] tempBytes;

                        lock (_lockObjectSaveFilePeace)
                        {
                            tempBytes = File.ReadAllBytes(tempFilePath);
                        }

                        outp = AppendArrays(outp, tempBytes);
                }

                lock (_lockObjectSaveFilePeace)
                {
                    using (var outputFileStream = new FileStream(outputFilePath, FileMode.Append))
                    {
                        outputFileStream.Write(outp, 0, outp.Length);
                    }
                }


                //using (var outputFileStream = new FileStream(outputFilePath, FileMode.Append))
                //{
                //    var orderedList = _filePeacesToOpen.OrderBy(x => x.OrderId);
                //    foreach (var filePeace in orderedList)
                //    {
                //        var tempFilePath = Path.Combine(filePeacesFolder, filePeace.Id.ToString());

                //        var tempBytes = File.ReadAllBytes(tempFilePath);

                //        outputFileStream.Write(tempBytes, 0, tempBytes.Length);
                //        outputFileStream.Seek(outputFileStream.Length, SeekOrigin.Begin);
                //    }
                //}
            }
        }

        private static byte[] AppendArrays(byte[] appendThis, byte[] appendWithThis)
        {
            var tempArray = new byte[appendThis.Length + appendWithThis.Length];

            int i = 0;

            while (i < appendThis.Length)
            {
                tempArray[i] = appendThis[i];
                i++;
            }

            for (int j = 0; j < appendWithThis.Length; j++)
            {
                int s = i + j;
                tempArray[s] = appendWithThis[j];
            }

            return tempArray;
        }

        private async Task<(List<(Guid Id, int OrderId)> MissingIds, List<(Guid Id, int OrderId)> AllIds)> 
            SendFileOpeningRequestAsync(Guid fileId)
        {
            var user = UserService.Instance.GetCurrentUser();

            var response = await _client.PostAsync(
                Configuration.OpenFileRequestRelativeEndpoint + "/" + fileId,
                new StringContent(JsonConvert.SerializeObject(user.Token1), Encoding.UTF8, "application/json"),
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new OperationFailedException("Failed to send open file request, API call unsuccessful: " + errorMessage);
            }

            var returnJson = await response.Content.ReadAsStringAsync();

           // (MissingIds: responsePrep, AllIds: relatedFilePeaces)
            return JsonConvert.DeserializeObject<(List<(Guid Id, int OrderId)> MissingIds, List<(Guid Id, int OrderId)> AllIds)>(returnJson);
        }

        public async Task<byte[]> GetBytesById(Guid filePieceId)
        {
            return await Task.Run(() =>
            {
                var directory = Path.Combine(Configuration.RootFolder, Configuration.FilePiecesFolderName);

                lock (_lockObjectSaveFilePeace)
                {
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }

                var filePath = Path.Combine(directory, filePieceId.ToString());

                if (!File.Exists(filePath))
                {
                    Task.Delay(1000);

                    if (!File.Exists(filePath))
                    {
                        Task.Delay(1000);

                        if (!File.Exists(filePath))
                        {
                            Task.Delay(1000);

                            if (!File.Exists(filePath))
                            {
                                Console.WriteLine($"File path not found {filePath}");

                                throw new OperationFailedException("The given file piece does not exists in this phone");
                            }
                        }
                    }
                }

                byte[] resp;

                lock (_lockObjectSaveFilePeace)
                {
                    resp = File.ReadAllBytes(filePath);
                }

                return resp;
            });
        }

        public async Task SendFilePieceToServerAsync(Guid id)
        {
            var fileBytes = await GetBytesById(id);

            var content = new MultipartFormDataContent();
            var byteArrayContent = new ByteArrayContent(fileBytes, 0, fileBytes.Length);
            content.Add(byteArrayContent, "filePieces", id.ToString());

            var response = await _client.PostAsync(
                Configuration.SendFilePieceRelativeEndpoint + "/" + id,
                content,
                CancellationToken.None);


            var respText = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                //var errorMessage = await response.Content.ReadAsStringAsync();
                //throw new OperationFailedException($"Failed to send file piece ({id}), API call unsuccessful: " + errorMessage);
            }
        }

        public async Task SaveFilePieceAsync(Guid fileId, byte[] fileBytes)
        {
            await Task.Run(() =>
            {
                var directory = Path.Combine(Configuration.RootFolder, Configuration.FilePiecesFolderName);

                lock (_lockObjectSaveFilePeace)
                {
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }

                var filePath = Path.Combine(directory, fileId.ToString());

                lock (_lockObjectSaveFilePeace)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                lock (_lockObjectSaveFilePeace)
                {
                    File.WriteAllBytes(filePath, fileBytes);
                }
            });
        }

        public async Task<long> DeleteFilePiece(Guid fileId)
        {
            var directory = Path.Combine(Configuration.RootFolder, Configuration.FilePiecesFolderName);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var filePath = Path.Combine(directory, fileId.ToString());

            if (File.Exists(filePath))
            {
                long fileSize = new System.IO.FileInfo(filePath).Length;

                File.Delete(filePath);

                return fileSize;
            }

            return 0;
        }

        public async Task<Result> UploadNewFileToServerAsync(string fileName, byte[] fileBytes, string mimeType)
        {
            var dto = new UploadFileDto
            {
                FileName = fileName,
                MimeType = mimeType,
                UserToken1 = UserService.Instance.GetCurrentUser().Token1
            };

            var content = new MultipartFormDataContent();

            var byteArrayContent = new ByteArrayContent(fileBytes, 0, fileBytes.Length);
            content.Add(byteArrayContent, "fileByte", fileName);

            var stringContent = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8);
            content.Add(stringContent, "dto");

            //_client.Timeout = TimeSpan.FromMinutes(10);

            var response = await _client.PostAsync(
                Configuration.UploadFileRelativeEndpoint,
                content,
                CancellationToken.None);


            var respText = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Unknown error.";

                try
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }
                catch (System.Exception)
                {
                }

                return new Result
                {
                    IsSuccess = false,
                    ErrorMessage = $"Failed to upload file. Error: {errorMessage}"
                };

            }

            return new Result {IsSuccess = true};
        }

        public async Task EmptyOpenableFolder()
        {
            var folder = Path.Combine(Configuration.RootFolder, Configuration.OpenableTempFolderName);

            lock (_lockObjectSaveFilePeace)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            foreach (var file in Directory.GetFiles(folder))
            {
                if (File.Exists(file) && File.GetCreationTimeUtc(file).AddMinutes(10) < DateTime.UtcNow)
                {
                    File.Delete(file);
                }
            }
        }

        public double GetOpenableFolderSize()
        {
            var folder = Path.Combine(Configuration.RootFolder, Configuration.OpenableTempFolderName);

            lock (_lockObjectSaveFilePeace)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);

                    return 0.0;
                }
            }

            long resp = 0;

            lock (_lockObjectSaveFilePeace)
            {
                foreach (var file in Directory.GetFiles(folder))
                {
                    if (File.Exists(file))
                    {
                        resp += new FileInfo(file).Length;
                    }
                }
            }

            double response = (double)resp / 1024 / 1024;

            return Math.Round(response, MidpointRounding.ToEven);
        }
    }
}

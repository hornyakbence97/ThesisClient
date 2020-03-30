using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using XamarinApp.Services;
using XamarinApp.WebSocket.Model;
using XamarinApp.WebSocket.Model.Dto.Input;
using XamarinApp.WebSocket.Model.Dto.Output;

namespace XamarinApp.WebSocket
{
    class WebSocketProcesser
    {
        private ClientWebSocket _clientWebSocket;
        private Object _lockObject = new object();
        private object _lockObjectRead = new object();
        private bool isPeriodicalCheckRunning = false;

        public WebSocketProcesser()
        {
            _clientWebSocket = new ClientWebSocket();
            _clientWebSocket.Options.KeepAliveInterval = Configuration.KeepAliveInterval;
            _clientWebSocket.Options.SetBuffer(Configuration.ReceiveBufferSize, Configuration.ReceiveBufferSize);
        }

        public async Task StartTask()
        {
            if (!isPeriodicalCheckRunning)
            {
                StartPeriodicallyCheck();
            }

            if (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open)
            {
                try
                {
                    await ConnectAndAuthenticate();

                    await VirtualFileService.Instance.SendFilesToServer();

                    await GoIdle();
                }
                catch (System.Exception e)
                {
                }
            }
        }

        private async Task StartPeriodicallyCheck()
        {
            isPeriodicalCheckRunning = true;

            try
            {
                if (_clientWebSocket.State == WebSocketState.Connecting)
                {
                    while (_clientWebSocket.State == WebSocketState.Connecting)
                    {
                        await Task.Delay(100);
                    }
                }
                //sending ping
                await WriteAsBinaryToWebSocket(new byte[0], _clientWebSocket, WebSocketMessageType.Binary);
            }
            catch (System.Exception e)
            {
                _clientWebSocket = new ClientWebSocket();
                _clientWebSocket.Options.KeepAliveInterval = Configuration.KeepAliveInterval;
                _clientWebSocket.Options.SetBuffer(
                    receiveBufferSize: Configuration.ReceiveBufferSize,
                    sendBufferSize: Configuration.ReceiveBufferSize);

                StartTask();
            }

            await Task.Delay(TimeSpan.FromSeconds(10));

            StartPeriodicallyCheck();
        }

        private async Task ConnectAndAuthenticate()
        {
            var user = UserService.Instance.GetCurrentUser();

            var authenticationDo = new AuthenticationDto
            {
                Token1 = user.Token1,
                FriendlyName = user.FriendlyName,
                RequestType = WebSocketRequestType.AUTHENTICATION,
                Token2 = user.Token2
            };

            await _clientWebSocket.ConnectAsync(Configuration.BaseUrlWebSocket, new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);

            await WriteStringToWebSocket(JsonConvert.SerializeObject(authenticationDo), _clientWebSocket);
        }

        private async Task GoIdle()
        {
            while (true)
            {
                var readResult = await ReadContentFromWebSocket(_clientWebSocket);

                switch (readResult.MessageType)
                {
                    case WebSocketMessageType.Text:
                        await ProcessIncomingRequest(readResult.Text);
                        break;
                    case WebSocketMessageType.Binary:
                        if (readResult.Bytes.Length > 0)
                        {
                            await ProcessSaveFileRequest(readResult.Bytes);
                        }

                        break;
                    case WebSocketMessageType.Close:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async Task ProcessIncomingRequest(string jsonText)
        {
            var baseDto = JsonConvert.DeserializeObject<IncomingBaseDto>(jsonText);

            switch (baseDto.RequestType)
            {
                case IncomingRequestType.SEND_FILE:
                    await ProcessSendFileRequest(jsonText);
                    break;
                case IncomingRequestType.DELETE_FILE:
                    await ProcessDeleteFileRequest(jsonText);
                    break;
                case IncomingRequestType.SAVE_FILE:
                    await ProcessSaveFileRequest(jsonText);
                    break;
                case IncomingRequestType.EMPTY_OPENABLE_FOLDER:
                    await ProcessEmptyOpenableFolderRequest();
                    break;
                case IncomingRequestType.REMOVE_FILE_PEACE:
                    await RemoveFilePeaceRequest(jsonText);
                    break;
                case IncomingRequestType.FETCH_FILES:
                    await SendFetchFilesRequest();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task SendFetchFilesRequest()
        {
            try
            {
                MessagingCenter.Send<EventsClass>(EventsClass.Instance, Events.Events.Fetch);
            }
            catch (System.Exception e)
            {
            }
        }

        private async Task RemoveFilePeaceRequest(string jsonText)
        {
            var dto = JsonConvert.DeserializeObject<DeleteFilePeaceDto>(jsonText);

            await VirtualFileService.Instance.DeleteFilePiece(dto.FilePeaceId);
        }

        private async Task ProcessEmptyOpenableFolderRequest()
        {
            await VirtualFileService.Instance.EmptyOpenableFolder();
        }

        private async Task ProcessSendFileRequest(string jsonText)
        {
            var dto = JsonConvert.DeserializeObject<SendFileDto>(jsonText);

            foreach (var id in dto.FilePieceIds)
            {
                await VirtualFileService.Instance.SendFilePieceToServerAsync(id);
            }
        }

        private async Task ProcessDeleteFileRequest(string jsonText)
        {
            var dto = JsonConvert.DeserializeObject<DeleteFileDto>(jsonText);

            foreach (var filePeaceId in dto.FilePiecesToDelete)
            {
                var size = await VirtualFileService.Instance.DeleteFilePiece(filePeaceId);

                await SendConfirm(filePeaceId, dto.RequestType, size);
            }
        }

        private async Task ProcessSaveFileRequest(string jsonText)
        {
            //todo figure out something else: sending file in DTO is not good
            var dto = JsonConvert.DeserializeObject<SaveFileDto>(jsonText);

            foreach (var filePeace in dto.FilePeaces)
            {
                await VirtualFileService.Instance.SaveFilePieceAsync(filePeace.Id, filePeace.Bytes);

                await SendConfirm(filePeace.Id, dto.RequestType);

                try
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        MessagingCenter.Send<string>(filePeace.Id.ToString(), Events.Events.WebSocketReceive);
                    });
                }
                catch (System.Exception e)
                {
                    Console.WriteLine(e);
                    //throw;
                }
            }
        }

        private async Task ProcessSaveFileRequest(byte[] bytesIncoming)
        {
            byte[] guidBytes = new byte[16];
            byte[] dataBytes = new byte[bytesIncoming.Length - guidBytes.Length];

            for (int i = 0; i < guidBytes.Length; i++)
            {
                guidBytes[i] = bytesIncoming[i];
            }

            int j = 0;
            for (int i = guidBytes.Length; i < bytesIncoming.Length; i++)
            {
                dataBytes[j] = bytesIncoming[i];
                j++;
            }

            var filePeaceId = new Guid(guidBytes);

            //#region Debug

            //var szoveg = string.Join(',', dataBytes);

            //var client = new HttpClient
            //{
            //    BaseAddress = Configuration.BaseUrl
            //};

            //var response = await client.PostAsync(
            //    "Files/DebugTxt",
            //    new StringContent(JsonConvert.SerializeObject(new {Id = filePeaceId.ToString(), Text = szoveg}), Encoding.UTF8, "application/json"),
            //    CancellationToken.None);

            //#endregion

            await VirtualFileService.Instance.SaveFilePieceAsync(filePeaceId, dataBytes);

            await SendConfirm(filePeaceId, IncomingRequestType.SAVE_FILE);

            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send<string>(filePeaceId.ToString(), Events.Events.WebSocketReceive);
                });
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
        }

        private async Task SendConfirm(Guid filePeaceId, IncomingRequestType webSocketRequestType, long fileSize = 0)
        {
            var dto = new ReceivedConfirmationDto
            {
                Token1 = UserService.Instance.GetCurrentUser().Token1,
                RequestType = WebSocketRequestType.RECEIVED_COMFIRMATION,
                ReceiveId = filePeaceId,
                Type = webSocketRequestType,
                FilePeaceSize = fileSize
            };

            await WriteStringToWebSocket(JsonConvert.SerializeObject(dto), _clientWebSocket);
        }

        private async Task WriteStringToWebSocket(string text, ClientWebSocket webSocket)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            await WriteAsBinaryToWebSocket(bytes, webSocket, WebSocketMessageType.Text);
        }

        private async Task WriteAsBinaryToWebSocket(byte[] bytes, ClientWebSocket webSocket, WebSocketMessageType type)
        {
            lock (_lockObject)
            {
                webSocket
                    .SendAsync(
                        new ArraySegment<byte>(bytes, 0, bytes.Length),
                        type,
                        true,
                        CancellationToken.None)
                    .Wait(TimeSpan.FromSeconds(5));
            }
        }

        private async Task<(WebSocketMessageType MessageType, byte[] Bytes, string Text)> ReadContentFromWebSocket(ClientWebSocket webSocket)
        {
            var readResult = await ReadBinaryContentFromWebSocket(webSocket);

            (WebSocketMessageType MessageType, byte[] Bytes, string Text) ret;

            ret = readResult.MessageType == WebSocketMessageType.Text
                ? (readResult.MessageType, readResult.Bytes, Encoding.UTF8.GetString(readResult.Bytes))
                : (readResult.MessageType, readResult.Bytes, string.Empty);

            return ret;
        }

        private async Task<(byte[] Bytes, WebSocketMessageType MessageType)> ReadBinaryContentFromWebSocket(ClientWebSocket webSocket)
        {
            lock (_lockObjectRead)
            {
                try
                {
                    var bufferArray = new byte[Configuration.ReceiveBufferSize];

                    var inputResult = webSocket
                        .ReceiveAsync(new ArraySegment<byte>(bufferArray), CancellationToken.None)
                        .Result;

                    var mainBuffer = new ArraySegment<byte>(bufferArray, 0, inputResult.Count).Array;

                    mainBuffer= CutZerosFromTheEnd(mainBuffer, inputResult.Count);

                    while (!inputResult.EndOfMessage)
                    {
                        bufferArray = new byte[Configuration.ReceiveBufferSize];

                        inputResult = webSocket
                            .ReceiveAsync(new ArraySegment<byte>(bufferArray), CancellationToken.None).Result;

                        var temporaryBuffer = new ArraySegment<byte>(bufferArray, 0, inputResult.Count).Array;

                        temporaryBuffer = CutZerosFromTheEnd(temporaryBuffer, inputResult.Count);

                        mainBuffer = AppendArrays(mainBuffer, temporaryBuffer);
                    }

                    return (mainBuffer, inputResult.MessageType);
                }
                catch (System.Exception e)
                {
                    return default;
                }
            }
        }

        private static byte[] CutZerosFromTheEnd(byte[] mainBuffer, int inputResultCount)
        {
            var response = new byte[inputResultCount];

            for (int i = 0; i < response.Length; i++)
            {
                response[i] = mainBuffer[i];
            }

            return response;
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
    }
}

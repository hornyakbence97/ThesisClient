using System;
using System.Collections.Generic;
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

        public WebSocketProcesser()
        {
            _clientWebSocket = new ClientWebSocket();
            _clientWebSocket.Options.KeepAliveInterval = Configuration.KeepAliveInterval;
            _clientWebSocket.Options.SetBuffer(Configuration.ReceiveBufferSize, Configuration.ReceiveBufferSize);
        }

        public async Task StartTask()
        {
            await ConnectAndAuthenticate();

            await GoIdle();
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

            await _clientWebSocket.ConnectAsync(Configuration.BaseUrlWebSocket, CancellationToken.None);

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
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                await VirtualFileService.Instance.DeleteFilePiece(filePeaceId);

                await SendConfirm(filePeaceId, dto.RequestType);
            }
        }

        private async Task ProcessSaveFileRequest(string jsonText)
        {
            var dto = JsonConvert.DeserializeObject<SaveFileDto>(jsonText);

            foreach (var filePeace in dto.FilePeaces)
            {
                await VirtualFileService.Instance.SaveFilePieceAsync(filePeace.Id, filePeace.Bytes);

                await SendConfirm(filePeace.Id, dto.RequestType);

                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send<string>(filePeace.Id.ToString(), Events.Events.WebSocketReceive);
                });
            }
        }

        private async Task SendConfirm(Guid filePeaceId, IncomingRequestType webSocketRequestType)
        {
            var dto = new ReceivedConfirmationDto
            {
                Token1 = UserService.Instance.GetCurrentUser().Token1,
                RequestType = WebSocketRequestType.RECEIVED_COMFIRMATION,
                ReceiveId = filePeaceId,
                Type = webSocketRequestType
            };

            await WriteStringToWebSocket(JsonConvert.SerializeObject(dto), _clientWebSocket);
        }

        private async Task WriteStringToWebSocket(string text, ClientWebSocket webSocket)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            await WriteAsBinaryToWebSocket(bytes, webSocket, WebSocketMessageType.Text);
        }

        private static async Task WriteAsBinaryToWebSocket(byte[] bytes, ClientWebSocket webSocket, WebSocketMessageType type)
        {
            if (bytes.Length <= Configuration.ReceiveBufferSize)
            {
                await webSocket
                    .SendAsync(
                        new ArraySegment<byte>(bytes, 0, bytes.Length),
                        type,
                        true,
                        CancellationToken.None);

                return;
            }

            var start = 0;
            var end = Configuration.ReceiveBufferSize;
            var bufferSize = Configuration.ReceiveBufferSize;
            var canContinue = true;

            while (canContinue)
            {
                var isEndOfMessage = false;

                if (end >= bytes.Length)
                {
                    var dif = end - bytes.Length;
                    bufferSize -= dif;

                    canContinue = false;

                    isEndOfMessage = true;
                }

                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes, start, bufferSize),
                    type,
                    isEndOfMessage,
                    CancellationToken.None);

                start = end;

                end = end + Configuration.ReceiveBufferSize;
            }
        }

        private static async Task<(WebSocketMessageType MessageType, byte[] Bytes, string Text)> ReadContentFromWebSocket(ClientWebSocket webSocket)
        {
            var readResult = await ReadBinaryContentFromWebSocket(webSocket);

            (WebSocketMessageType MessageType, byte[] Bytes, string Text) ret;

            ret = readResult.MessageType == WebSocketMessageType.Text
                ? (readResult.MessageType, readResult.Bytes, Encoding.UTF8.GetString(readResult.Bytes))
                : (readResult.MessageType, readResult.Bytes, string.Empty);

            return ret;
        }

        private static async Task<(byte[] Bytes, WebSocketMessageType MessageType)> ReadBinaryContentFromWebSocket(ClientWebSocket webSocket)
        {
            var bufferArray = new byte[Configuration.ReceiveBufferSize];

            var inputResult = await webSocket
                .ReceiveAsync(new ArraySegment<byte>(bufferArray), CancellationToken.None);

            var mainBuffer = new ArraySegment<byte>(bufferArray, 0, inputResult.Count).Array;

            while (!inputResult.EndOfMessage)
            {
                bufferArray = new byte[Configuration.ReceiveBufferSize];

                inputResult = await webSocket
                    .ReceiveAsync(new ArraySegment<byte>(bufferArray), CancellationToken.None);

                var temporaryBuffer = new ArraySegment<byte>(bufferArray, 0, inputResult.Count).Array;

                mainBuffer = AppendArrays(mainBuffer, temporaryBuffer);
            }

            return (mainBuffer, inputResult.MessageType);
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

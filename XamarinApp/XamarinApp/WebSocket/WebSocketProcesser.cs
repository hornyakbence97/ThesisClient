using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinApp.WebSocket
{
    class WebSocketProcesser
    {
        private ClientWebSocket _clientWebSocket;

        public WebSocketProcesser()
        {
            _clientWebSocket = new ClientWebSocket();
            _clientWebSocket.Options.KeepAliveInterval = Configuration.KeepAliveInterval;
        }

        public async Task StartTask()
        {
            await _clientWebSocket.ConnectAsync(Configuration.BaseUrlWebSocket, CancellationToken.None);

            await WriteStringToWebSocket("{\"Token1\": \"9c25a537-a309-4edd-8186-8ad6c2d4c907\", \"Token2\": \"3fd4cdf0-0d34-4ff4-9509-4136ed48b9f3\", \"RequestType\": 1, \"ReceiveId\": \"3fd4cdf0-0d34-4ff4-9509-4136ed48b9f4\"}", _clientWebSocket);
            await WriteStringToWebSocket("{\"Token1\": \"9c25a537-a309-4edd-8186-8ad6c2d4c907\", \"Token2\": \"3fd4cdf0-0d34-4ff4-9509-4136ed48b9f3\", \"RequestType\": 2, \"ReceiveId\": \"3fd4cdf0-0d34-4ff4-9509-4136ed48b9f4\"}", _clientWebSocket);


            while (true)
            {
                var text = await ReadStringContentFromWebSocket(_clientWebSocket);

                Device.BeginInvokeOnMainThread(() =>
                {
                    MessagingCenter.Send<string>(_clientWebSocket.State.ToString() + text, Events.Events.WebSocketReceive);
                });
            }
        }

        private async Task WriteStringToWebSocket(string text, ClientWebSocket webSocket)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            await WriteAsBinaryToWebSocket(bytes, webSocket, WebSocketMessageType.Text);
        }

        private static async Task WriteAsBinaryToWebSocket(byte[] bytes, ClientWebSocket webSocket, WebSocketMessageType type = WebSocketMessageType.Text)
        {
            if (bytes.Length <= Configuration.ReceiveBufferSize)
            {
                await webSocket
                    .SendAsync(
                        new ArraySegment<byte>(bytes, 0, bytes.Length),
                        WebSocketMessageType.Text,
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

        private static async Task<string> ReadStringContentFromWebSocket(ClientWebSocket webSocket)
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

            var ret =  Encoding.UTF8.GetString(mainBuffer);

            Console.WriteLine(ret);

            return ret;
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

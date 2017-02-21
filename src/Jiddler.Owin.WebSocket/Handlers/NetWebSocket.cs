using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Jiddler.Owin.WebSocket.Extensions;

namespace Jiddler.Owin.WebSocket.Handlers {
    internal class NetWebSocket : IWebSocket {
        private readonly TaskQueue _sendQueue;
        private readonly System.Net.WebSockets.WebSocket _webSocket;

        public NetWebSocket(System.Net.WebSockets.WebSocket webSocket) {
            _webSocket = webSocket;
            _sendQueue = new TaskQueue();
        }

        public TaskQueue SendQueue => _sendQueue;
        public WebSocketCloseStatus? CloseStatus => _webSocket.CloseStatus;
        public string CloseStatusDescription => _webSocket.CloseStatusDescription;

        public Task SendText(ArraySegment<byte> data, bool endOfMessage, CancellationToken cancelToken) {
            return Send(data, WebSocketMessageType.Text, endOfMessage, cancelToken);
        }

        public Task SendBinary(ArraySegment<byte> data, bool endOfMessage, CancellationToken cancelToken) {
            return Send(data, WebSocketMessageType.Binary, endOfMessage, cancelToken);
        }

        public Task Send(ArraySegment<byte> data, WebSocketMessageType messageType, bool endOfMessage,
            CancellationToken cancelToken) {
            var sendContext = new NetSendContext(data, endOfMessage, messageType, cancelToken);

            return _sendQueue.Enqueue(
                async s => { await _webSocket.SendAsync(s.Buffer, s.MessageType, s.EndOfMessage, s.CancellationToken); },
                sendContext);
        }

        public Task Close(WebSocketCloseStatus closeStatus, string closeDescription, CancellationToken cancelToken) {
            return _webSocket.CloseAsync(closeStatus, closeDescription, cancelToken);
        }

        public async Task<Tuple<ArraySegment<byte>, WebSocketMessageType>> ReceiveMessage(byte[] buffer,
            CancellationToken cancelToken) {
            var count = 0;
            WebSocketReceiveResult result;
            do {
                var segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                result = await _webSocket.ReceiveAsync(segment, cancelToken);

                count += result.Count;
            } while (!result.EndOfMessage);

            return new Tuple<ArraySegment<byte>, WebSocketMessageType>(new ArraySegment<byte>(buffer, 0, count),
                result.MessageType);
        }
    }
}
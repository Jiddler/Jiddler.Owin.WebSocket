using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Jiddler.Owin.WebSocket.Extensions;

namespace Jiddler.Owin.WebSocket.Handlers {
    using WebSocketSendAsync = Func<ArraySegment<byte> /* data */, int /* messageType */, bool /* endOfMessage */, CancellationToken /* cancel */, Task>;
    using WebSocketReceiveAsync = Func<ArraySegment<byte> /* data */,CancellationToken /* cancel */,Task<Tuple<int /* messageType */,bool /* endOfMessage */,int /* count */>>>;
    using WebSocketCloseAsync = Func<int /* closeStatus */, string /* closeDescription */, CancellationToken /* cancel */, Task>;

    internal class OwinWebSocket : IWebSocket {
        internal const int CONTINUATION_OP = 0x0;
        internal const int TEXT_OP = 0x1;
        internal const int BINARY_OP = 0x2;
        internal const int CLOSE_OP = 0x8;
        internal const int PONG_OP = 0xA;
        internal const int PING_OP = 0x9;

        private readonly WebSocketSendAsync _sendAsync;
        private readonly WebSocketReceiveAsync _receiveAsync;
        private readonly WebSocketCloseAsync _closeAsync;
        private readonly TaskQueue _sendQueue;

        public TaskQueue SendQueue => _sendQueue;
        public WebSocketCloseStatus? CloseStatus => null;
        public string CloseStatusDescription => null;

        public OwinWebSocket(IDictionary<string, object> owinEnvironment) {
            _sendAsync = (WebSocketSendAsync) owinEnvironment["websocket.SendAsync"];
            _receiveAsync = (WebSocketReceiveAsync) owinEnvironment["websocket.ReceiveAsync"];
            _closeAsync = (WebSocketCloseAsync) owinEnvironment["websocket.CloseAsync"];
            _sendQueue = new TaskQueue();
        }

        public Task SendText(ArraySegment<byte> data, bool endOfMessage, CancellationToken cancelToken) {
            return Send(data, WebSocketMessageType.Text, endOfMessage, cancelToken);
        }

        public Task SendBinary(ArraySegment<byte> data, bool endOfMessage, CancellationToken cancelToken) {
            return Send(data, WebSocketMessageType.Binary, endOfMessage, cancelToken);
        }

        public Task Send(ArraySegment<byte> data, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancelToken) {
            return Enqueue(data, MessageTypeEnumToOpCode(messageType), endOfMessage, cancelToken);
        }

        private Task Enqueue(ArraySegment<byte> data, int opCode, bool endOfMessage, CancellationToken cancelToken) {
            var sendContext = new OwinSendContext(data, endOfMessage, opCode, cancelToken);

            return _sendQueue.Enqueue(async s => { await _sendAsync(s.Buffer, s.OpCode, s.EndOfMessage, s.CancellationToken); }, sendContext);
        }

        public Task Close(WebSocketCloseStatus closeStatus, string closeDescription, CancellationToken cancelToken) {
            return _closeAsync((int) closeStatus, closeDescription, cancelToken);
        }

        public async Task<Tuple<ArraySegment<byte>, WebSocketMessageType>> ReceiveMessage(byte[] buffer, CancellationToken cancelToken) {
            var count = 0;
            Tuple<int, bool, int> result;
            int opType = -1;
            do {
                var segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                result = await _receiveAsync(segment, cancelToken);

                count += result.Item3;
                if (opType == -1)
                    opType = result.Item1;

                if (count == buffer.Length && !result.Item2)
                    throw new InternalBufferOverflowException("The Buffer is to small to get the Websocket Message! Increase in the Constructor!");
            } while (!result.Item2);

            if (opType == PING_OP) // PING
            {
                await Enqueue(new ArraySegment<byte>(new byte[0]), PONG_OP, true, CancellationToken.None);
            }

            return new Tuple<ArraySegment<byte>, WebSocketMessageType>(new ArraySegment<byte>(buffer, 0, count), MessageTypeOpCodeToEnum(opType));
        }

        private static WebSocketMessageType MessageTypeOpCodeToEnum(int messageType) {
            switch (messageType) {
                case TEXT_OP:
                    return WebSocketMessageType.Text;
                case BINARY_OP:
                    return WebSocketMessageType.Binary;
                case CLOSE_OP:
                    return WebSocketMessageType.Close;
                case PONG_OP:
                    return WebSocketMessageType.Binary;
                case PING_OP:
                    return WebSocketMessageType.Binary;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, string.Empty);
            }
        }

        private static int MessageTypeEnumToOpCode(WebSocketMessageType webSocketMessageType) {
            switch (webSocketMessageType) {
                case WebSocketMessageType.Text:
                    return TEXT_OP;
                case WebSocketMessageType.Binary:
                    return BINARY_OP;
                case WebSocketMessageType.Close:
                    return CLOSE_OP;
                default:
                    throw new ArgumentOutOfRangeException(nameof(webSocketMessageType), webSocketMessageType, string.Empty);
            }
        }
    }
}
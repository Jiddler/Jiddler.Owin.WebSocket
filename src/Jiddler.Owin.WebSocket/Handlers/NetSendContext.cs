using System;
using System.Net.WebSockets;
using System.Threading;

namespace Jiddler.Owin.WebSocket.Handlers {
    internal class NetSendContext : AbstractSendContext
    {
        public WebSocketMessageType MessageType { get; }

        public NetSendContext(ArraySegment<byte> buffer, bool endOfMessage, WebSocketMessageType messageType, CancellationToken cancellationToken) : base(buffer, endOfMessage, cancellationToken)
        {
            MessageType = messageType;
        }
    }
}
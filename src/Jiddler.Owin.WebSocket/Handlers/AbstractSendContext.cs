using System;
using System.Threading;

namespace Jiddler.Owin.WebSocket.Handlers {
    internal abstract class AbstractSendContext {
        public ArraySegment<byte> Buffer { get; }
        public bool EndOfMessage { get; }
        public CancellationToken CancellationToken { get; }

        protected AbstractSendContext(ArraySegment<byte> buffer, bool endOfMessage, CancellationToken cancellationToken) {
            Buffer = buffer;
            EndOfMessage = endOfMessage;
            CancellationToken = cancellationToken;
        }
    }
}
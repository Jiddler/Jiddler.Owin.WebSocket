using System;
using System.Threading;

namespace Jiddler.Owin.WebSocket.Handlers {
    internal class OwinSendContext : AbstractSendContext
    {
        public int OpCode { get; }

        public OwinSendContext(ArraySegment<byte> buffer, bool endOfMessage, int opCode, CancellationToken cancellationToken) : base(buffer, endOfMessage, cancellationToken)
        {
            OpCode = opCode;
        }
    }
}
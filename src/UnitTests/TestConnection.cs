using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Jiddler.Owin.WebSocket;
using Microsoft.Owin;

namespace UnitTests {
    [WebSocketRoute("/wsa")]
    class TestConnection : WebSocketConnection {
        public ArraySegment<byte> LastMessage { get; set; }
        public WebSocketMessageType LastMessageType { get; set; }
        public bool OnOpenCalled { get; set; }
        public bool OnOpenAsyncCalled { get; set; }
        public bool OnCloseCalled { get; set; }
        public bool OnCloseAsyncCalled { get; set; }
        public IOwinRequest Request { get; set; }

        public WebSocketCloseStatus? CloseStatus { get; set; }
        public string CloseDescription { get; set; }

        public WebSocketCloseStatus? AsyncCloseStatus { get; set; }
        public string AsyncCloseDescription { get; set; }

        public override async Task OnMessageReceived(ArraySegment<byte> message, WebSocketMessageType type) {
            LastMessage = message;
            LastMessageType = type;

            //Echo it back
            await Send(message, true, type);
        }

        public override void OnOpen() {
            OnOpenCalled = true;
        }

        public override Task OnOpenAsync() {
            OnOpenAsyncCalled = true;
            return Task.Delay(0);
        }

        public override void OnClose(WebSocketCloseStatus? closeStatus, string closeStatusDescription) {
            OnCloseCalled = true;
            CloseStatus = closeStatus;
            CloseDescription = closeStatusDescription;
        }

        public override Task OnCloseAsync(WebSocketCloseStatus? closeStatus, string closeStatusDescription) {
            OnCloseAsyncCalled = true;
            AsyncCloseStatus = closeStatus;
            AsyncCloseDescription = closeStatusDescription;
            return Task.Delay(0);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Practices.ServiceLocation;

namespace Jiddler.Owin.WebSocket {
    public class WebSocketConnectionMiddleware<T> : OwinMiddleware where T : WebSocketConnection {
        private readonly Regex _matchPattern;
        private readonly IServiceLocator _serviceLocator;

        public WebSocketConnectionMiddleware(OwinMiddleware next, IServiceLocator locator)
            : base(next) {
            _serviceLocator = locator;
        }

        public WebSocketConnectionMiddleware(OwinMiddleware next, IServiceLocator locator, Regex matchPattern)
            : this(next, locator) {
            _matchPattern = matchPattern;
        }

        public override Task Invoke(IOwinContext context) {
            var matches = new Dictionary<string, string>();

            if (_matchPattern != null) {
                var match = _matchPattern.Match(context.Request.Path.Value);
                if (!match.Success)
                    return Next.Invoke(context);

                for (var i = 1; i <= match.Groups.Count; i++) {
                    var name = _matchPattern.GroupNameFromNumber(i);
                    var value = match.Groups[i];
                    matches.Add(name, value.Value);
                }
            }

            var socketConnection = _serviceLocator == null ? Activator.CreateInstance<T>() : _serviceLocator.GetInstance<T>();

            return socketConnection.AcceptSocketAsync(context, matches);
        }
    }
}
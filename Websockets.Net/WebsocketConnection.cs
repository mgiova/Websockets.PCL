using System;
using System.Collections.Generic;

namespace Websockets.Net
{
    /// <summary>
    /// A Websocket connection for 'full' .Net Core applications
    /// </summary>
    public class WebsocketConnection : IWebSocketConnection
    {
        public bool IsOpen { get; private set; }

        public event Action OnClosed = delegate { };
        public event Action OnOpened = delegate { };
        public event Action<IWebSocketConnection> OnDispose = delegate { };
        public event Action<string> OnError = delegate { };
        public event Action<string> OnMessage = delegate { };
        public event Action<string> OnLog = delegate { };

        static WebsocketConnection()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => true;
        }

        /// <summary>
        /// Factory Initializer
        /// </summary>
        public static void Link()
        {
            WebSocketFactory.Init(() => new WebsocketConnection());
        }

        private WebSocketWrapper _websocket = null;
        private Dictionary<string, string> httpheaders = new Dictionary<string, string>();

        public async void Open(string url, string protocol = null)
        {
            try
            {
                if (_websocket != null)
                    EndConnection();

                _websocket = new WebSocketWrapper();
                _websocket.Closed += _websocket_Closed;
                _websocket.Opened += _websocket_Opened;
                _websocket.Error += _websocket_Error;
                _websocket.MessageReceived += _websocket_MessageReceived;

                if (url.StartsWith("https"))
                    url = url.Replace("https://", "wss://");
                else if (url.StartsWith("http"))
                    url = url.Replace("http://", "ws://");

                foreach (var header in httpheaders)
                    _websocket.SetRequestHeader(header.Key, header.Value);

                await _websocket.Connect(url, protocol);

            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        public void AddHttpHeader(string header, string value)
        {
            httpheaders.Add(header, value);
        }

        public void Close()
        {
            EndConnection();
        }

        public async void Send(string message)
        {
            await _websocket.SendMessage(message);
        }


        public void Dispose()
        {
            Close();
            OnDispose(this);
        }

        //
        void EndConnection()
        {
            if (_websocket != null)
            {
                _websocket.Closed -= _websocket_Closed;
                _websocket.Opened -= _websocket_Opened;
                _websocket.Error -= _websocket_Error;
                _websocket.MessageReceived -= _websocket_MessageReceived;
                _websocket.Dispose();
                _websocket = null;

                IsOpen = false;
                OnClosed();
            }
        }


        void _websocket_Error(Exception obj)
        {
            OnError(obj.Message);
        }

        void _websocket_Opened(WebSocketWrapper arg)
        {

            IsOpen = true;
            OnOpened();
        }

        void _websocket_MessageReceived(string m, WebSocketWrapper arg)
        {

            OnMessage(m);
        }

        void _websocket_Closed(WebSocketWrapper arg)
        {
            EndConnection();
        }
    }
}
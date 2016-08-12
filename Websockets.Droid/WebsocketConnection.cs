using System;
using System.Collections.Generic;
using Websockets.DroidBridge;

namespace Websockets.Droid
{
    /// <summary>
    /// A Websocket connection via androidBridge application
    /// </summary>
    public class WebsocketConnection : BridgeProxy, IWebSocketConnection
    {
        public bool IsOpen { get; private set; }

        public event Action OnClosed = delegate { };
        public event Action OnOpened = delegate { };
        public event Action<IWebSocketConnection> OnDispose = delegate { };
        public event Action<string> OnError = delegate { };
        public event Action<string> OnMessage = delegate { };
        public event Action<byte[]> OnData = delegate { };
        public event Action<string> OnLog = delegate { };

        private BridgeController _controller;
        private Dictionary<string, string> httpheaders = new Dictionary<string, string>();

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

        public void Close()
        {
            try
            {
                IsOpen = false;
                _controller.Close();
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        public void Open(string url, string protocol = null)
        {
            try
            {
                _controller = new BridgeController();
                _controller.Proxy = this;
                _controller.Proxy = this;
                _controller.Proxy = this;

                foreach (var header in httpheaders)
                    _controller.SetRequestHeader(header.Key, header.Value);

                _controller.Open(url, protocol);
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

        protected override void Dispose(bool disposing)
        {
            try
            {
                Close();
                OnDispose(this);
                base.Dispose(disposing);
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }


        public void Send(string message)
        {
            try
            {
                _controller.Send(message);

            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

        //

        public override unsafe void RaiseClosed()
        {
            IsOpen = false;
            OnClosed();
            base.RaiseClosed();
        }

        public override unsafe void RaiseError(string p1)
        {
            OnError(p1);
            base.RaiseError(p1);
        }

        public override unsafe void RaiseLog(string p1)
        {
            OnLog(p1);
            base.RaiseLog(p1);
        }

        public override unsafe void RaiseMessage(string p1)
        {
            OnMessage(p1);
            base.RaiseMessage(p1);
        }

        public override unsafe void RaiseOpened()
        {
            IsOpen = true;
            OnOpened();
            base.RaiseOpened();
        }
    }
}
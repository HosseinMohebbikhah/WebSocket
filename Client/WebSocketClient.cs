using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal class WebSocketClient : IDisposable
    {
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;

        public WebSocketClient()
        {
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync(string url)
        {
            while (true)
            {
                try
                {
                    ChangedStatus($"Connecting...");
                    await _webSocket.ConnectAsync(new Uri(url), _cancellationTokenSource.Token);
                    ChangedStatus($"Connected");
                    Connected();
                    await Task.WhenAll(ReceiveAsync());
                }
                catch (Exception ex)
                {
                    ChangedStatus($"The connection lost...");
                    _webSocket.Dispose();
                    _webSocket = new ClientWebSocket();
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                ChangedStatus("Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        private async Task ReceiveAsync()
        {
            byte[] buffer = new byte[1024];

            while (_webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    DataReceived(message);
                }
            }
        }

        public async Task SendAsyncMessage(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            if (_webSocket.State == WebSocketState.Open)
                await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _webSocket.Dispose();
        }

        public event Action<string> OnDataReceived;

        protected virtual void DataReceived(string data)
        {
            OnDataReceived?.Invoke(data);
        }

        public event Action OnConnected;
        protected virtual void Connected()
        {
            OnConnected?.Invoke();
        }

        public event Action<string> OnChangedStatus;
        protected virtual void ChangedStatus(string status)
        {
            OnChangedStatus?.Invoke(status);
        }
    }
}

using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    internal class WebSocketServer : IDisposable
    {
        private HttpListener _httpListener;
        private CancellationTokenSource _cancellationTokenSource;
        private Action<string, WebSocket> _dataReceivedCallback;

        public WebSocketServer()
        {
            ChangedStatus("Initializing...");
            _httpListener = new HttpListener();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        //public async Task StartAsync(string url, Action<string, WebSocket> dataReceivedCallback)
        public async Task StartAsync(string url)
        {
            _httpListener.Prefixes.Add(url);
            _httpListener.Start();

            //_dataReceivedCallback = dataReceivedCallback;
            ChangedStatus("Listening...");
            await Task.Run(() => ListenAsync(_cancellationTokenSource.Token));
        }

        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                HttpListenerContext context = await _httpListener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                    ChangedStatus("Connected. (The existence of the client)");
                    _ = Task.Run(() => ReceiveAsync(webSocketContext.WebSocket, cancellationToken));
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task ReceiveAsync(WebSocket webSocket, CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    DataReceived(message, webSocket);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                }
            }
        }

        public async Task SendAsync(WebSocket webSocket, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _httpListener.Stop();
            _httpListener.Close();
        }

        public event Action<string, WebSocket> OnDataReceived;

        protected virtual void DataReceived(string data, WebSocket webSocket)
        {
            OnDataReceived?.Invoke(data, webSocket);
        }

        public event Action<string> OnChangedStatus;

        protected virtual void ChangedStatus(string data)
        {
            OnChangedStatus?.Invoke(data);
        }
    }
}

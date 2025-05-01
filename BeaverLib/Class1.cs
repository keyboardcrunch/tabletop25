using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Principal;
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
    }

    public static class ShortcutsThroughGraveyards
    {

    }
}
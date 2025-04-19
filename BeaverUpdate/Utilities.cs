using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace beaverUpdate
{
    internal class Utilities
    {
        private static Mutex mutex = null;

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving parent process: {ex.Message}");
                return null;
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
}

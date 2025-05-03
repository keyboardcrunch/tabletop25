using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;

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
                        if (request == "notepad")
                        {
                            RunCmd("notepad.exe", new string[0], true);
                        }
                    }
                }
            }
        }

        public static void RunCmd(string process, string[] args, bool shell)
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
    }
}
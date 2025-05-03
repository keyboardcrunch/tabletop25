using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;

namespace BeaverPipeTest
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            await SendRequest(args[0]);
        }

        public static async Task SendRequest(string command)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "BeaverOnThePipe", PipeDirection.InOut))
            {
                Console.WriteLine("Connecting to server...");
                await pipeClient.ConnectAsync(); // Use ConnectAsync for asynchronous connection
                Console.WriteLine("Connected to server.");
                Console.WriteLine($"Sending: {command}");

                using (StreamWriter writer = new StreamWriter(pipeClient))
                {
                    await writer.WriteAsync(command); // Use WriteAsync for asynchronous writing
                    await writer.FlushAsync(); // Ensure the data is written
                }
            }
        }
    }
}

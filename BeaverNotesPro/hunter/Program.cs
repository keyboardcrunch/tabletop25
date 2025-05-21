using System;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace hunter
{
    internal class Program
    {
        // Constants from lmerr.h
        private const int NERR_Success = 0;
        private const int ERROR_NOT_ENOUGH_MEMORY = 8;
        private const int ERROR_INVALID_PARAMETER = 87;
        private const int ERROR_NO_BROWSER_SERVERS_FOUND = 6118;

        // Structures and enums from lmsname.h
        private enum NetJoinStatus
        {
            NetSetupUnknown,
            NetSetupMemberServer,
            NetSetupStandAlone,
            NetSetupWorkgroup,
            NetSetupDomainBDC,
            NetSetupDomainPrimary
        }

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int NetGetJoinInformation(
            string serverName,
            out IntPtr domainName,
            out NetJoinStatus joinStatus);

        [DllImport("Netapi32.dll")]
        private static extern int NetApiBufferFree(IntPtr buffer);

        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                string query = args[0];
                string[] keywords = query.Split(',');

                IntPtr domainNamePtr;
                NetJoinStatus joinStatus;

                // Get the domain name of the computer
                int result = NetGetJoinInformation(null, out domainNamePtr, out joinStatus);

                if (result == NERR_Success)
                {
                    try
                    {
                        string dnsdomain = Environment.GetEnvironmentVariable("USERDNSDOMAIN");
                        string[] splitdomain = dnsdomain.Split('.');
                        string searchBase = $"DC={splitdomain[0]},DC={splitdomain[1]}";

                        try
                        {
                            // Construct the LDAP path to query Active Directory
                            string ldapPath = $"LDAP://{searchBase}";
                            using (DirectoryEntry entry = new DirectoryEntry(ldapPath))
                            using (DirectorySearcher searcher = new DirectorySearcher(entry))
                            {
                                // Filter to find groups with any of the specified keywords in their name
                                searcher.Filter = "(objectClass=group)";
                                searcher.PropertiesToLoad.Add("name");

                                // Find all matching groups
                                foreach (SearchResult groupResult in searcher.FindAll())
                                {
                                    string groupName = groupResult.Properties["name"][0].ToString();

                                    // Check if the group name contains any of the keywords
                                    foreach (var keyword in keywords)
                                    {
                                        if (groupName.Contains(keyword))
                                        {
                                            Console.WriteLine($"Found group: {groupName}");

                                            using (DirectoryEntry groupEntry = new DirectoryEntry(groupResult.Path))
                                            using (DirectorySearcher memberSearcher = new DirectorySearcher(groupEntry))
                                            {
                                                memberSearcher.Filter = "(objectClass=user)";
                                                memberSearcher.PropertiesToLoad.Add("displayName");
                                                memberSearcher.PropertiesToLoad.Add("mail");
                                                memberSearcher.PropertiesToLoad.Add("description");
                                                memberSearcher.PropertiesToLoad.Add("department");
                                                memberSearcher.PropertiesToLoad.Add("manager");

                                                try
                                                {
                                                    foreach (SearchResult memberResult in memberSearcher.FindAll())
                                                    {
                                                        Console.WriteLine("Found a result for: " + memberResult.Properties["displayName"][0]);

                                                        Console.WriteLine($"Username: {memberResult.Properties["sAMAccountName"][0]}");
                                                        Console.WriteLine($"Display Name: {memberResult.Properties["displayName"][0]}");
                                                        if (memberResult.Properties.Contains("mail"))
                                                            Console.WriteLine($"Email: {memberResult.Properties["mail"][0]}");
                                                        if (memberResult.Properties.Contains("description"))
                                                            Console.WriteLine($"Description: {memberResult.Properties["description"][0]}");
                                                        if (memberResult.Properties.Contains("department"))
                                                            Console.WriteLine($"Department: {memberResult.Properties["department"][0]}");
                                                        if (memberResult.Properties.Contains("manager"))
                                                            Console.WriteLine($"Manager: {memberResult.Properties["manager"][0]}");

                                                        Console.WriteLine("----------------------------------------");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine($"Error processing search results: {ex.Message}");
                                                    Console.WriteLine(ex.StackTrace);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error during LDAP query: {ex.Message}");
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                    finally
                    {
                        NetApiBufferFree(domainNamePtr);
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to get domain information. Error code: {result}");
                }
            }
        }
    }
}
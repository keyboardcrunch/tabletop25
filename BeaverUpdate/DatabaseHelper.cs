using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BeaverUpdate
{
    public class DatabaseManager
    {
        private string _databasePath;
        private SQLiteConnection _connection;
        private const string DATABASE_PASSWORD = "voidserpent";

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
                    DataSource = _databasePath,
                    Password = DATABASE_PASSWORD
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
                    size INTEGER NOT NULL,
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

        public void FileEntry(string name, string path, string sha1, long size, bool reported = false, bool uploaded = false)
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
                        size,
                        reported,
                        uploaded
                    ) VALUES (
                        (SELECT id FROM files WHERE sha1 = @sha1),
                        @name,
                        @path,
                        @sha1,
                        @size,
                        @reported,
                        @uploaded
                    );";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@path", path);
                    command.Parameters.AddWithValue("@sha1", sha1);
                    command.Parameters.AddWithValue("@size", size);
                    command.Parameters.AddWithValue("@reported", reported ? 1 : 0);
                    command.Parameters.AddWithValue("@uploaded", uploaded ? 1 : 0);

                    command.ExecuteNonQuery();
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
                DataSource = _databasePath,
                Password = DATABASE_PASSWORD
            };

            return connectionStringBuilder.ConnectionString;
        }
    }
}
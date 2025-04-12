using System;
using System.Data.SQLite;

namespace beaverUpdate
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Data Source=updates.db;Version=3;";

        public static void EnsureTablesExist()
        {
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();

                string sql = @"
                    CREATE TABLE IF NOT EXISTS updates (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                    );
                    CREATE TABLE IF NOT EXISTS scans (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        scan_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                        result TEXT
                    );
                    CREATE TABLE IF NOT EXISTS emails (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT,
                        email TEXT,
                        sent_date DATETIME DEFAULT CURRENT_TIMESTAMP
                    );
                    CREATE TABLE IF NOT EXISTS tasks (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        task_name TEXT,
                        status TEXT,
                        created_at DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void LogRunEvent()
        {
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();

                string sql = "INSERT INTO updates (timestamp) VALUES (@timestamp);";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static DateTime GetLastRunTimestamp()
        {
            using (SQLiteConnection conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();

                string sql = "SELECT timestamp FROM updates ORDER BY timestamp DESC LIMIT 1;";

                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToDateTime(result);
                    }
                }
            }

            // Return a default value (e.g., DateTime.MinValue) if no record is found
            return DateTime.Now;
        }



    }
}
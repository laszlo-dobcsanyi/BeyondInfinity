using System;
using System.Data.SQLite;

namespace BeyondInfinity_Server
{
    public static class DatabaseManager
    {
        private static SQLiteConnection Connection;

        public static void Connect()
        {
            try
            {
                Connection = new SQLiteConnection("Data Source=database.s3db");
                Connection.Open();

                Connection.Close();
            }
            catch (Exception E)
            {
                Console.WriteLine("Error while connecting to database!\n" + E.Message);
            }
        }
    }
}

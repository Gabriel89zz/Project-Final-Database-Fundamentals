using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project_Final_Database_Fundamentals
{
    internal class DatabaseConnection
    {
        private const string ConnectionString = "Host=192.168.1.173;Port=5432;Database=Football;Username=postgres;Password=bankai";

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }
    }
}

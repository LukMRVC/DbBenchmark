using System;
using System.Data;
using System.IO;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class DatabaseConnection : IDisposable
    {
        private NpgsqlConnection Connection { get; set; }
        private NpgsqlTransaction Transaction { get; set; }
        public string Language = "en";
        private bool EveryInTransaction = false;
        
        public DatabaseConnection()
        {
            Connection = new NpgsqlConnection();
        }

        public DatabaseConnection(bool runEveryInTransaction = false)
        {
            Connection = new NpgsqlConnection();
            EveryInTransaction = runEveryInTransaction;
        }

        public bool Connect(string connectionString)
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.ConnectionString = connectionString;
                try
                {
                    Connection.Open();
                }
                catch (NpgsqlException)
                {
                    return false;
                }
            }

            return true;
        }

        private static string GoUpNLevels(int levels, string path)
        {
            string newPath = (string) path.Clone();
            for (int i = 0; i < levels; ++i)
            {
                newPath = Path.GetDirectoryName(newPath);
            }

            return newPath;
        }

        public bool Connect()
        {
            if (Connection.State != ConnectionState.Open)
            {
                var builder = new ConfigurationBuilder();
                builder.AddJsonFile(Path.Combine(GoUpNLevels(4, Path.GetFullPath("./")), "appsettings.json"));
                var root = builder.Build();

                return Connect(root.GetConnectionString("DefaultConnection"));
            }

            return true;
        }

        public void Close()
        {
            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }

        public void BeginTransaction()
        {
            Transaction = Connection.BeginTransaction(IsolationLevel.Serializable);
        }

        public void Commit()
        {
            Transaction.Commit();
            Connection.Close();
        }

        public void Rollback()
        {
            Transaction?.Rollback();
        }

        public int Execute(NpgsqlCommand command)
        {
            var rowNumber = 0;
            try
            {
                rowNumber = command.ExecuteNonQuery();
            }
            catch (NpgsqlException e)
            {
                throw e;
            }

            return rowNumber;
        }

        public NpgsqlCommand Command(string command)
        {
            if (EveryInTransaction)
            {
                BeginTransaction();
            }
            
            var com = new NpgsqlCommand(command, Connection);

            if (Transaction != null)
            {
                com.Transaction = Transaction;
            }

            return com;
        }

        public NpgsqlCommand AssignTransaction(NpgsqlCommand command)
        {
            if (Transaction != null)
            {
                command.Transaction = Transaction;
            }

            return command;
        }

        public NpgsqlDataReader Select(NpgsqlCommand command)
        {
            return command.ExecuteReader();
        }

        public void Dispose()
        {
            Transaction?.Rollback();
            if (Connection.State == ConnectionState.Open)
            {
                Connection.Close();
            }
            
            Connection?.Dispose();
            Transaction?.Dispose();
        }
    }
}
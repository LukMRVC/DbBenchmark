using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using DbBenchmark.ORM.DAO;
using DbBenchmark.ORM.DTO;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class ContractTable
    {
        private static readonly string TableName = "contract";

        //funkce 1.5
        private static readonly string SQL_SELECT = $"SELECT * FROM {TableName} WHERE deleted_at IS NULL";
        
        //funkce 1.5
        private static readonly string SQL_SELECT_PAGED = $"SELECT * FROM {TableName} WHERE deleted_at IS NULL " +
                                                          $"OFFSET @offset FETCH NEXT @psize ROWS ONLY";

        //funkce 1.4
        private static readonly string SQL_SELECT_ID =
            $"SELECT * FROM {TableName} WHERE contract_id=@contract_id AND deleted_at IS NULL";

        //funkce 1.1
        private static readonly string SQL_INSERT =
            $"INSERT INTO {TableName} VALUES (@variable_symbol, @contract_name, @identification_number," +
            $" @vat_identification_number, CURRENT_TIMESTAMP, NULL, @notify_limit, @email, @phone_number, @bonus_amount)";

        //funkce 1.3
        private static readonly string SQL_DELETE_ID = $"DELETE FROM {TableName} WHERE contract_id=@contract_id";

        //funkce 1.2
        private static readonly string SQL_UPDATE =
            $"UPDATE {TableName} SET variable_symbol=@variable_symbol, contract_name=@contract_name," +
            $" identification_number=@identification_number," +
            $" vat_identification_number=@vat_identification_number, notify_limit=@notify_limit," +
            $" email=@email, phone_number=@phone_number, bonus_amount=@bonus_amount WHERE contract_id=@contract_id";

        private static readonly string SQL_LAST_ID = $"SELECT MAX(contract_id) FROM {TableName}";


        //funkce 1.1
        public static int Insert(Contract contract, DatabaseConnection connection = null)
        {
            DatabaseConnection db;
            if (connection == null)
            {
                db = new DatabaseConnection();
            }
            else
            {
                db = connection;
            }

            db.Connect();
            var command = db.Command(SQL_INSERT);
            PrepareCommand(command, contract);
            int ret = db.Execute(command);
            if (ret > 0)
            {
                contract.Id = LastId(db);
            }

            if (connection == null)
                db.Close();
            return ret;
        }


        public static int LastId(DatabaseConnection connection = null)
        {
            DatabaseConnection db;
            if (connection == null)
            {
                db = new DatabaseConnection();
            }
            else
            {
                db = connection;
            }

            db.Connect();
            var command = db.Command(SQL_LAST_ID);
            int lastId = 0;
            using (var reader = db.Select(command))
            {
                if (reader.Read())
                {
                    lastId = reader.GetInt32(0);
                    reader.Close();
                }
            }

            if (connection == null)
                db.Close();
            return lastId;
        }


        //funkce 1.2

        public static int Update(Contract address, DatabaseConnection connection = null)
        {
            DatabaseConnection db;
            if (connection == null)
            {
                db = new DatabaseConnection();
            }
            else
            {
                db = connection;
            }

            db.Connect();
            var command = db.Command(SQL_UPDATE);
            PrepareCommand(command, address);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 1.4
        public static Collection<Contract> Select(bool relationIgnore = false, DatabaseConnection connection = null)
        {
            DatabaseConnection db;
            if (connection == null)
            {
                db = new DatabaseConnection();
            }
            else
            {
                db = connection;
            }

            db.Connect();

            var command = db.Command(SQL_SELECT);
            NpgsqlDataReader reader = db.Select(command);

            Collection<Contract> contracts = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return contracts;
        }
        
        //funkce 1.4
        public static Collection<Contract> SelectPaged(int page = 0, int pageSize = 50, 
            bool relationIgnore = false, DatabaseConnection connection = null)
        {
            DatabaseConnection db;
            if (connection == null)
            {
                db = new DatabaseConnection();
            }
            else
            {
                db = connection;
            }

            db.Connect();

            var command = db.Command(SQL_SELECT_PAGED);
            var offset = pageSize * (page - 1);
            command.Parameters.AddWithValue("psize", pageSize);
            command.Parameters.AddWithValue("offset", offset);
            NpgsqlDataReader reader = db.Select(command);
            Collection<Contract> contracts = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return contracts;
        }

        //DB trigger, funkce 1.3
        public static int Delete(Contract contract, DatabaseConnection connection = null)
        {
            DatabaseConnection db;
            if (connection == null)
            {
                db = new DatabaseConnection();
            }
            else
            {
                db = connection;
            }

            db.Connect();
            var command = db.Command(SQL_DELETE_ID);
            command.Parameters.AddWithValue("@contract_id", contract.Id);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 1.4
        public static Contract Select(int id, bool relationIgnore = false, DatabaseConnection connection = null)
        {
            DatabaseConnection db;
            if (connection == null)
            {
                db = new DatabaseConnection();
            }
            else
            {
                db = connection;
            }

            db.Connect();

            var command = db.Command(SQL_SELECT_ID);
            command.Parameters.AddWithValue("@contract_id", id);
            NpgsqlDataReader reader = db.Select(command);
            Collection<Contract> contracts = Read(reader, relationIgnore);
            Contract contract = null;
            if (contracts.Count > 0)
            {
                contract = contracts[0];
            }

            reader.Close();
            if (connection == null)
                db.Close();
            return contract;
        }

        private static Collection<Contract> Read(NpgsqlDataReader reader, bool relationIgnore)
        {
            Collection<Contract> addresses = new();
            while (reader.Read())
            {
                Contract contract = new Contract();
                contract.Id = (int) reader["contract_id"];
                contract.VariableSymbol = (int) reader["variable_symbol"];
                contract.ContractName = (string) reader["contract_name"];
                if (!reader.IsDBNull("identification_number"))
                {
                    contract.IdentificationNumber = (int) reader["identification_number"];
                }

                if (!reader.IsDBNull("vat_identification_number"))
                {
                    contract.VatIdentificationNumber = (string) reader["vat_identification_number"];
                }

                contract.CreatedAt = (DateTime) reader["created_at"];
                if (!reader.IsDBNull("notify_limit"))
                {
                    contract.NotifyLimit = Decimal.ToDouble((Decimal) reader["notify_limit"]);
                }

                contract.Email = (string) reader["email"];
                contract.PhoneNumber = (string) reader["phone_number"];
                if (!reader.IsDBNull("bonus_amount"))
                {
                    contract.BonusAmount = Decimal.ToDouble((Decimal) reader["bonus_amount"]);
                }

                if (!relationIgnore)
                {
                    contract.Address = AddressTable.SelectForContract(contract);
                    contract.Invoices = InvoiceTable.SelectForContract(contract);
                    contract.Participants = ParticipantTable.SelectForContract(contract);
                }

                addresses.Add(contract);
            }

            return addresses;
        }

        private static void PrepareCommand(NpgsqlCommand command, Contract contract)
        {
            command.Parameters.AddWithValue("@contract_id", contract.Id);
            command.Parameters.AddWithValue("@variable_symbol", contract.VariableSymbol);
            command.Parameters.AddWithValue("@contract_name", contract.ContractName);
            command.Parameters.AddWithValue("@identification_number",
                (object) contract.IdentificationNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@vat_identification_number",
                (object) contract.VatIdentificationNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@notify_limit", (object) contract.NotifyLimit ?? DBNull.Value);
            command.Parameters.AddWithValue("@email", contract.Email);
            command.Parameters.AddWithValue("@phone_number", contract.PhoneNumber);
            command.Parameters.AddWithValue("@bonus_amount", (object) contract.BonusAmount ?? DBNull.Value);
        }
    }
}
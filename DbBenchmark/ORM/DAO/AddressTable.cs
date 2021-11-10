using System.Collections.ObjectModel;
using System.Data;
using DbBenchmark.ORM.DAO;
using DbBenchmark.ORM.DTO;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class AddressTable
    {
        private static readonly string TableName = "dais.address";

        //funkce 2.4
        private static readonly string SQL_SELECT = $"SELECT * FROM {TableName}";

        //funkce 2.5
        private static readonly string SQL_SELECT_ID = $"SELECT * FROM {TableName} WHERE address_id=@address_id";

        //funkce 2.1
        private static readonly string SQL_INSERT =
            $"INSERT INTO {TableName} VALUES (@city, @district, @street_name, @house_number, @zip_code, @contract_id)";

        //funkce 2.3
        private static readonly string SQL_DELETE_ID = $"DELETE FROM {TableName} WHERE address_id=@address_id";

        //funkce 2.2
        private static readonly string SQL_UPDATE =
            $"UPDATE {TableName} SET city=@city, district=@district, street_name=@street_name," +
            $" house_number=@house_number, zip_code=@zip_code, contract_id=@contract_id " +
            $"WHERE address_id=@address_id";


        //funkce 2.4
        private static readonly string
            SQL_SELECT_CONTRACT = $"SELECT * FROM {TableName} WHERE contract_id=@contract_id";

        private static readonly string SQL_SELECT_LAST_ID = $"SELECT MAX(address_id) FROM {TableName}";

        //funkce 2.3
        public static int Delete(Address address, DatabaseConnection connection = null)
        {
            if (address.CanBeDeleted && address.Contract != null)
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
                command.Parameters.AddWithValue("@address_id", address.Id);
                int ret = db.Execute(command);
                if (connection == null)
                    db.Close();
                return ret;
            }

            return 0;
        }

        //funkce 2.1
        public static int Insert(Address address, DatabaseConnection connection = null)
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
            PrepareCommand(command, address);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            if (ret > 0)
            {
                address.Id = SelectLastId(db);
            }

            return ret;
        }

        //funkce 2.2
        public static int Update(Address address, DatabaseConnection connection = null)
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

        public static int SelectLastId(DatabaseConnection connection = null)
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

            var command = db.Command(SQL_SELECT_LAST_ID);
            int lastId = 0;
            using (var reader = db.Select(command))
            {
                if (reader.Read())
                {
                    lastId = reader.GetInt32(0);
                }
            }

            return lastId;
        }

        //funkce 2.4
        public static Collection<Address> Select(bool relationIgnore = false, DatabaseConnection connection = null)
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
            var reader = db.Select(command);

            Collection<Address> addresses = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return addresses;
        }

        //funkce 2.5
        public static Address Select(int id, bool relationIgnore = false, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@address_id", id);
            var reader = db.Select(command);
            Collection<Address> addresses = Read(reader, relationIgnore);
            Address address = null;
            if (addresses.Count > 0)
            {
                address = addresses[0];
            }

            reader.Close();
            if (connection == null)
                db.Close();
            return address;
        }

        public static Address SelectForContract(Contract contract, DatabaseConnection connection = null)
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

            var command = db.Command(SQL_SELECT_CONTRACT);
            command.Parameters.AddWithValue("@contract_id", contract.Id);
            var reader = db.Select(command);
            Collection<Address> addresses = Read(reader, true);
            Address address = null;
            if (addresses.Count > 0)
            {
                address = addresses[0];
            }

            reader.Close();
            if (connection == null)
                db.Close();
            return address;
        }

        private static Collection<Address> Read(NpgsqlDataReader reader, bool relationIgnore)
        {
            Collection<Address> addresses = new Collection<Address>();
            while (reader.Read())
            {
                Address a = new Address();
                a.Id = (int) reader["address_id"];
                a.City = (string) reader["city"];
                a.District = (string) reader["district"];
                a.StreetName = (string) reader["street_name"];
                a.HouseNumber = (int) reader["house_number"];
                a.ZipCode = (int) reader["zip_code"];
                if (!reader.IsDBNull("contract_id"))
                    a.ContractId = (int) reader["contract_id"];
                if (!relationIgnore)
                    a.Contract = a.ContractId.HasValue ? ContractTable.Select(a.ContractId.Value, true) : null;
                addresses.Add(a);
            }

            return addresses;
        }

        private static void PrepareCommand(NpgsqlCommand command, Address address)
        {
            command.Parameters.AddWithValue("@address_id", address.Id);
            command.Parameters.AddWithValue("@city", address.City);
            command.Parameters.AddWithValue("@district", address.District);
            command.Parameters.AddWithValue("@street_name", address.StreetName);
            command.Parameters.AddWithValue("@house_number", address.HouseNumber);
            command.Parameters.AddWithValue("@zip_code", address.ZipCode);
            command.Parameters.AddWithValue("@contract_id", address.ContractId);
        }
    }
}
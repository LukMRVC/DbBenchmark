using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using DbBenchmark.ORM.DTO;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class PriceListTable
    {
        private static readonly string TableName = "price_list";

        //funkce 6.4
        private static readonly string SQL_SELECT = $"SELECT * FROM {TableName}";
        private static readonly string SQL_SELECT_ID = $"SELECT * FROM {TableName} WHERE price_list_id=@price_list_id";

        //funkce 6.1
        private static readonly string SQL_INSERT =
            $"INSERT INTO {TableName} VALUES (@tarification_first, @tarification_second, " +
            $" @price_per_second, @phone_country_code)";

        //funkce 6.3
        private static readonly string SQL_DELETE_ID = $"DELETE FROM {TableName} WHERE price_list_id=@price_list_id";

        //funkce 6.2
        private static readonly string SQL_UPDATE = $"UPDATE {TableName} SET tarification_first=@tarification_first," +
                                                    $" tarification_second=@tarification_second," +
                                                    $" price_per_second=@price_per_second, phone_country_code=@phone_country_code " +
                                                    $"WHERE price_list_id=@price_list_id";

        private static readonly string SQL_LAST_ID = $"SELECT MAX(price_list_id) FROM {TableName}";


        //funkce 6.1
        public static int Insert(PriceList priceList, DatabaseConnection connection = null)
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
            PrepareCommand(command, priceList);
            int ret = db.Execute(command);
            if (ret > 0)
            {
                priceList.Id = SelectLastId(db);
            }

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
            var command = db.Command(SQL_LAST_ID);
            int lastId = 0;
            using (var reader = db.Select(command))
            {
                if (reader.Read())
                {
                    lastId = reader.GetInt32(0);
                }
            }

            if (connection == null)
                db.Close();
            return lastId;
        }

        //funkce 6.2
        public static int Update(PriceList priceList, DatabaseConnection connection = null)
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
            PrepareCommand(command, priceList);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 6.3
        public static int Delete(PriceList priceList, DatabaseConnection connection = null)
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
            var col = CallDetailRecordTable.SelectForPriceList(priceList, false, db);
            if (col.Count > 0)
            {
                throw new Exception("Record cannot be deleted because CDR records exists");
            }

            var command = db.Command(SQL_DELETE_ID);
            command.Parameters.AddWithValue("@price_list_id", priceList.Id);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 6.4
        public static Collection<PriceList> Select(bool relationIgnore = false, DatabaseConnection connection = null)
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

            Collection<PriceList> addresses = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return addresses;
        }

        public static PriceList Select(int id, bool relationIgnore = false, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@price_list_id", id);
            var reader = db.Select(command);
            Collection<PriceList> priceLists = Read(reader, relationIgnore);
            PriceList priceList = null;
            if (priceLists.Count > 0)
            {
                priceList = priceLists[0];
            }

            reader.Close();
            if (connection == null)
                db.Close();
            return priceList;
        }

        private static Collection<PriceList> Read(NpgsqlDataReader reader, bool relationIgnore)
        {
            Collection<PriceList> priceLists = new Collection<PriceList>();
            while (reader.Read())
            {
                PriceList priceList = new PriceList();
                priceList.Id = (int) reader["price_list_id"];
                priceList.TarificationFirst = (int) reader["tarification_first"];
                priceList.TarificationSecond = (int) reader["tarification_second"];
                priceList.PricePerSecond = (int) reader["price_per_second"];
                priceList.PhoneCountryCode = (int) reader["phone_country_code"];
                if (!relationIgnore)
                    priceList.CallDetailRecord = CallDetailRecordTable.SelectForPriceList(priceList, true);
                priceLists.Add(priceList);
            }

            return priceLists;
        }

        public static void PrepareCommand(NpgsqlCommand command, PriceList priceList)
        {
            command.Parameters.AddWithValue("@price_list_id", priceList.Id);
            command.Parameters.AddWithValue("@tarification_first", priceList.TarificationFirst);
            command.Parameters.AddWithValue("@tarification_second", priceList.TarificationSecond);
            command.Parameters.AddWithValue("@price_per_second", priceList.PricePerSecond);
            command.Parameters.AddWithValue("@phone_country_code", priceList.PhoneCountryCode);
        }
    }
}
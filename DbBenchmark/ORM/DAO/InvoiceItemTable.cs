using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using DbBenchmark.ORM.DTO;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class InvoiceItemTable
    {
        private static readonly string TableName = "dais.invoice_item";

        //funkce 8.3
        private static readonly string SQL_SELECT = $"SELECT * FROM {TableName}";

        //funkce 8.4
        private static readonly string SQL_SELECT_ID = $"SELECT * FROM {TableName} WHERE item_id=@item_id";

        //funkce 8.1
        private static readonly string SQL_INSERT = $"INSERT INTO {TableName} VALUES (@name, @cost)";

        //funkce 8.2
        private static readonly string SQL_UPDATE = $"UPDATE {TableName} SET unit_cost=@cost, item_name=@name" +
                                                    $" WHERE item_id=@item_id";


        private static readonly string SQL_LAST_ID = $"SELECT MAX(item_id) FROM {TableName}";

        private static void PrepareCommand(NpgsqlCommand command, InvoiceItem invoiceItem)
        {
            command.Parameters.AddWithValue("@item_id", invoiceItem.Id);
            command.Parameters.AddWithValue("@cost", invoiceItem.UnitCost);
            command.Parameters.AddWithValue("@name", invoiceItem.Name);
        }

        //funkce 8.1
        public static int Insert(InvoiceItem invoiceItem, DatabaseConnection connection = null)
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
            PrepareCommand(command, invoiceItem);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            if (ret > 0)
            {
                invoiceItem.Id = SelectLastId(db);
            }

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
            var reader = db.Select(command);
            int lastId = 0;
            if (reader.Read())
            {
                lastId = reader.GetInt32(0);
            }

            return lastId;
        }

        //funkce 8.2
        public static int Update(InvoiceItem invoiceItem, DatabaseConnection connection = null)
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
            PrepareCommand(command, invoiceItem);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 8.3
        public static Collection<InvoiceItem> Select(bool relationIgnore = false, DatabaseConnection connection = null)
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

            Collection<InvoiceItem> invoiceItems = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return invoiceItems;
        }

        //funkce 8.4
        public static InvoiceItem Select(int id, bool relationIgnore = false, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@item_id", id);
            var reader = db.Select(command);
            Collection<InvoiceItem> invoiceItems = Read(reader, relationIgnore);
            InvoiceItem invoiceItem = null;
            if (invoiceItems.Count > 0)
            {
                invoiceItem = invoiceItems[0];
            }

            reader.Close();
            if (connection == null)
                db.Close();
            return invoiceItem;
        }

        private static Collection<InvoiceItem> Read(NpgsqlDataReader reader, bool relationIgnore)
        {
            Collection<InvoiceItem> invoiceItems = new Collection<InvoiceItem>();
            while (reader.Read())
            {
                InvoiceItem invoiceItem = new InvoiceItem();
                invoiceItem.Id = (int) reader["item_id"];
                invoiceItem.Name = (string) reader["item_name"];
                invoiceItem.UnitCost = Decimal.ToDouble((Decimal) reader["unit_cost"]);
                if (!relationIgnore)
                {
                    invoiceItem.Invoices = InvoiceHasItemTable.SelectForItem(invoiceItem);
                }

                invoiceItems.Add(invoiceItem);
            }

            return invoiceItems;
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using DbBenchmark.ORM.DTO;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class InvoiceHasItemTable
    {
        private static readonly string TableName = "invoice_has_items";
        private static readonly string SQL_SELECT = $"SELECT * FROM {TableName}";
        private static readonly string SQL_SELECT_INVOICE = $"SELECT * FROM {TableName} WHERE invoice_number=@invoice";
        private static readonly string SQL_SELECT_ITEM = $"SELECT * FROM {TableName} WHERE invoice_item_id=@item";

        private static readonly string SQL_INSERT = $"INSERT INTO {TableName} VALUES (@invoice, @item, @cost, @count)";

        private static readonly string SQL_DELETE_ID =
            $"DELETE FROM {TableName} WHERE invoice_number=@invoice AND invoice_item_id=@item";

        private static readonly string SQL_UPDATE =
            $"UPDATE {TableName} SET item_unit_cost=@item, item_count=@count WHERE " +
            $"invoice_number=@invoice AND invoice_item_id=@item";

        public static void PrepareCommand(NpgsqlCommand command, InvoiceHasItem invoiceHasItem)
        {
            command.Parameters.AddWithValue("@invoice", invoiceHasItem.InvoiceNumber);
            command.Parameters.AddWithValue("@item", invoiceHasItem.InvoiceItemId);
            command.Parameters.AddWithValue("@cost", invoiceHasItem.ItemUnitCost);
            command.Parameters.AddWithValue("@count", invoiceHasItem.ItemCount);
        }

        public static int Insert(InvoiceHasItem invoice, DatabaseConnection connection = null)
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
            PrepareCommand(command, invoice);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        public static int Update(InvoiceHasItem invoice, DatabaseConnection connection = null)
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
            PrepareCommand(command, invoice);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        public static int Delete(InvoiceHasItem invoice, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@number", invoice.Invoice.Number);
            command.Parameters.AddWithValue("@item", invoice.InvoiceItem.Id);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        public static Collection<InvoiceHasItem> Select(bool relationIgnore = false,
            DatabaseConnection connection = null)
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

            Collection<InvoiceHasItem> invoices = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return invoices;
        }

        public static Collection<InvoiceHasItem> SelectForInvoice(Invoice invoice, bool relationIgnore = false,
            DatabaseConnection connection = null)
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

            var command = db.Command(SQL_SELECT_INVOICE);
            command.Parameters.AddWithValue("@invoice", invoice.Number);
            var reader = db.Select(command);

            Collection<InvoiceHasItem> invoices = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return invoices;
        }

        public static Collection<InvoiceHasItem> SelectForItem(InvoiceItem invoiceItem, bool relationIgnore = false,
            DatabaseConnection connection = null)
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

            var command = db.Command(SQL_SELECT_ITEM);
            command.Parameters.AddWithValue("@item", invoiceItem.Id);
            var reader = db.Select(command);

            Collection<InvoiceHasItem> invoiceItems = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return invoiceItems;
        }

        private static Collection<InvoiceHasItem> Read(NpgsqlDataReader reader, bool relationIgnore)
        {
            Collection<InvoiceHasItem> invoices = new Collection<InvoiceHasItem>();
            while (reader.Read())
            {
                InvoiceHasItem invoice = new InvoiceHasItem();
                invoice.InvoiceNumber = (int) reader["invoice_number"];
                invoice.InvoiceItemId = (int) reader["invoice_item_id"];
                if (!relationIgnore)
                {
                    invoice.Invoice = InvoiceTable.Select(invoice.InvoiceNumber);
                    invoice.InvoiceItem = InvoiceItemTable.Select(invoice.InvoiceItemId);
                }

                invoice.ItemUnitCost = Decimal.ToDouble((Decimal) reader["item_unit_cost"]);
                invoice.ItemCount = (int) reader["item_count"];
                invoices.Add(invoice);
            }

            return invoices;
        }
    }
}
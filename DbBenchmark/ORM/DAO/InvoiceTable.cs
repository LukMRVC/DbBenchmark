using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using DbBenchmark.ORM.DTO;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class InvoiceTable
    {
        private static readonly string TableName = "dais.invoice";

        //funkce 7.3
        private static readonly string SQL_SELECT = $"SELECT * FROM {TableName}";
        
        //funkce 7.3
        private static readonly string SQL_SELECT_PAGED = $"SELECT * FROM {TableName} OFFSET @offset FETCH NEXT @psize ROWS ONLY";
        
        private static readonly string SQL_SELECT_ID = $"SELECT * FROM {TableName} WHERE invoice_number=@number";

        //funkce 7.1
        private static readonly string SQL_INSERT = $"INSERT INTO {TableName} VALUES (@number, @amount, @tax," +
                                                    $" CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @maturity, NULL, @contract)";

        //funkce 7.2
        private static readonly string SQL_UPDATE = $"UPDATE {TableName} SET amount=@amount, contract_id=@contract," +
                                                    $" tax_value_percent=@tax, taxable_period=@period, maturity=@maturity," +
                                                    $" paid=@paid WHERE invoice_number=@number";

        private static readonly string SQL_MAX_NUMBER = $"SELECT MAX(invoice_number) FROM {TableName}";

        private static readonly string SQL_SELECT_CONTRACT = $"SELECT * FROM {TableName} WHERE contract_id=@contract";
        private static readonly string SQL_MONTHLY_INVOICE = $"EXEC dais.MonthlyInvoice";


        //funkce 10.2
        public static int MonthlyInvoice(DatabaseConnection connection = null)
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
            var command = db.Command(SQL_MONTHLY_INVOICE);
            int ret = db.Execute(command);
            return ret;
        }

        public static int SelectMaxNumber(DatabaseConnection connection = null)
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

            int num = 0;
            var command = db.Command(SQL_MAX_NUMBER);
            using (var reader = db.Select(command))
            {
                if (reader.Read())
                {
                    num = reader.GetInt32(0);
                }
            }

            return num;
        }

        public static Collection<Invoice> SelectForContract(Contract contract, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@contract", contract.Id);
            var reader = db.Select(command);

            Collection<Invoice> invoices = Read(reader, true);
            reader.Close();
            if (connection == null)
                db.Close();
            return invoices;
        }

        public static void PrepareCommand(NpgsqlCommand command, Invoice invoice)
        {
            command.Parameters.AddWithValue("@number", invoice.Number);
            command.Parameters.AddWithValue("@amount", invoice.Amount);
            command.Parameters.AddWithValue("@tax", invoice.TaxValuePercent);
            command.Parameters.AddWithValue("@period", invoice.TaxablePeriod);
            command.Parameters.AddWithValue("@maturity", invoice.Maturity);
            command.Parameters.AddWithValue("@paid", (object) invoice.Paid ?? DBNull.Value);
            command.Parameters.AddWithValue("@contract", invoice.ContractId);
        }

        //funkce 7.1
        public static int Insert(Invoice invoice, DatabaseConnection connection = null)
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
            db.BeginTransaction();
            int ret = 0;
            try
            {
                var command = db.Command(SQL_INSERT);
                PrepareCommand(command, invoice);
                ret = db.Execute(command);
                foreach (InvoiceHasItem invoiceHasItem in invoice.Items)
                {
                    invoiceHasItem.Invoice = invoice;
                    InvoiceHasItemTable.Insert(invoiceHasItem, db);
                }

                db.Commit();
            }
            catch (Exception e)
            {
                db.Rollback();
                throw e;
            }

            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 7.2
        public static int Update(Invoice invoice, DatabaseConnection connection = null)
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

        //funkce 7.3
        public static Collection<Invoice> Select(bool relationIgnore = false, DatabaseConnection connection = null)
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
            Collection<Invoice> invoices = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return invoices;
        }

        public static Collection<Invoice> SelectPaged(int page = 0, int pageSize = 50,
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
            var offset = pageSize * (page - 1);
            var command = db.Command(SQL_SELECT_PAGED);
            command.Parameters.AddWithValue("@psize", pageSize);
            command.Parameters.AddWithValue("@offset", offset);
            var reader = db.Select(command);
            Collection<Invoice> invoices = Read(reader, true);
            reader.Close();
            if (connection == null)
                db.Close();
            return invoices;
        }

        public static Invoice Select(int id, bool relationIgnore = false, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@number", id);
            var reader = db.Select(command);
            Collection<Invoice> invoices = Read(reader, relationIgnore);
            Invoice invoice = null;
            if (invoices.Count > 0)
            {
                invoice = invoices[0];
            }

            reader.Close();
            if (connection == null)
                db.Close();
            return invoice;
        }

        private static Collection<Invoice> Read(NpgsqlDataReader reader, bool relationIgnore)
        {
            Collection<Invoice> invoices = new Collection<Invoice>();
            while (reader.Read())
            {
                Invoice invoice = new Invoice();
                invoice.Number = (int) reader["invoice_number"];
                invoice.Amount = Decimal.ToDouble((Decimal) reader["amount"]);
                invoice.TaxValuePercent = (int) reader["tax_value_percent"];
                invoice.CreatedAt = (DateTime) reader["created_at"];
                invoice.TaxablePeriod = (DateTime) reader["taxable_period"];
                invoice.Maturity = (DateTime) reader["maturity"];
                if (!reader.IsDBNull("paid"))
                    invoice.Paid = (DateTime) reader["paid"];
                invoice.ContractId = (int) reader["contract_id"];
                if (!relationIgnore)
                {
                    invoice.Contract = ContractTable.Select(invoice.ContractId, true);
                    invoice.Items = InvoiceHasItemTable.SelectForInvoice(invoice);
                }

                invoices.Add(invoice);
            }

            return invoices;
        }
    }
}
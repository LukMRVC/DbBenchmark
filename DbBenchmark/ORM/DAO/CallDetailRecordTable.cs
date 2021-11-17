using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using DbBenchmark.ORM.DTO;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class CallDetailRecordTable
    {
        private static readonly string TableName = "dais.call_detail_record";

        private static readonly string SQL_SELECT = $"SELECT * FROM {TableName}";
        
        // funkce 5.3
        private static readonly string SQL_SELECT_PARTICIPANT =
            $"SELECT * FROM {TableName} WHERE number_id IN (SELECT number_id " +
            $"FROM voip_number WHERE participant_id=@participant";

        //funkce 5.4
        private static readonly string SQL_SELECT_ID = $"SELECT * FROM {TableName} WHERE call_id=@call_id";

        //funkce 5.1
        private static readonly string SQL_INSERT =
            $"EXEC dais.AddCallDetailRecord @sourceNum, @destNum, @callDate, @length, @disposition";

        //funkce 5.2
        private static readonly string SQL_UPDATE =
            $"UPDATE {TableName} SET source_num=@sourceNum, destinatiom_num=@destNum, callDate=@callDate " +
            $"length=@length, disposition=@disposition, price_list_id=@priceList, number_id=@number, " +
            $"incoming_outgoing=@incomingOutgoing WHERE call_id=@call_id";

        private static readonly string SQL_SELECT_NUMBER = $"SELECT * FROM {TableName} WHERE number_id=@number";

        private static readonly string SQL_SELECT_PRICE_LIST =
            $"SELECT * FROM {TableName} WHERE price_list_id=@priceList";


        //funkce 5.2
        public static int Update(CallDetailRecord callDetailRecord, DatabaseConnection connection = null)
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
            PrepareCommand(command, callDetailRecord);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 5.1
        public static int Insert(CallDetailRecord callDetailRecord, DatabaseConnection connection = null)
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
            PrepareCommand(command, callDetailRecord);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        public static Collection<CallDetailRecord> Select(bool relationIgnore = false,
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

            Collection<CallDetailRecord> callDetailRecords = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return callDetailRecords;
        }

        //funkce 5.4
        public static CallDetailRecord Select(int id, bool relationIgnore = false, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@call_id", id);
            var reader = db.Select(command);
            Collection<CallDetailRecord> callDetailRecords = Read(reader, relationIgnore);
            CallDetailRecord callDetailRecord = null;
            if (callDetailRecords.Count > 0)
            {
                callDetailRecord = callDetailRecords[0];
            }

            reader.Close();
            if (connection == null)
                db.Close();
            return callDetailRecord;
        }

        public static Collection<CallDetailRecord> SelectForNumber(VoipNumber voipNumber, bool relationIgnore = false,
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

            var command = db.Command(SQL_SELECT_NUMBER);
            command.Parameters.AddWithValue("@number", voipNumber.Id);
            var reader = db.Select(command);
            Collection<CallDetailRecord> callDetailRecords = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return callDetailRecords;
        }

        public static Collection<CallDetailRecord> SelectForParticipant(Participant participant, DatabaseConnection db = null)
        {
            DatabaseConnection connection;
            if (db == null)
            {
                connection = new DatabaseConnection();
            }
            else
            {
                connection = db;
            }

            connection.Connect();
            var command = db.Command(SQL_SELECT_PARTICIPANT);
            command.Parameters.AddWithValue("@participant", participant.Id);
            var reader = db.Select(command);
            var callDetailRecords = Read(reader, true);
            reader.Close();
            if (db == null)
                connection.Close();
            return callDetailRecords;
        }

        public static Collection<CallDetailRecord> SelectForPriceList(PriceList priceList, bool relationIgnore = false,
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
            var command = db.Command(SQL_SELECT_PRICE_LIST);
            command.Parameters.AddWithValue("@priceList", priceList.Id);
            var reader = db.Select(command);
            Collection<CallDetailRecord> callDetailRecords = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return callDetailRecords;
        }

        private static Collection<CallDetailRecord> Read(NpgsqlDataReader reader, bool relationIgnore)
        {
            Collection<CallDetailRecord> callDetailRecords = new Collection<CallDetailRecord>();
            while (reader.Read())
            {
                CallDetailRecord record = new CallDetailRecord();
                record.Id = (long) reader["call_id"];
                record.Disposition = (string) reader["disposition"];
                record.SourceNum = (string) reader["source_num"];
                record.DestinationNum = (string) reader["destination_num"];
                record.Length = (int) reader["length"];
                record.CallDate = (DateTime) reader["call_date"];
                record.VoipNumberId = (int) reader["number_id"];
                record.IncomingOutgoing = (bool) reader["incoming_outgoing"];
                if (!reader.IsDBNull("price_list_id"))
                    record.PriceListId = (int) reader["price_list_id"];
                if (!relationIgnore)
                {
                    record.VoipNumber = VoipNumberTable.Select(record.VoipNumberId);
                    record.PriceList = PriceListTable.Select(record.PriceListId);
                }

                callDetailRecords.Add(record);
            }

            return callDetailRecords;
        }

        public static void PrepareCommand(NpgsqlCommand command, CallDetailRecord callDetailRecord)
        {
            command.Parameters.AddWithValue("@call_id", callDetailRecord.Id);
            command.Parameters.AddWithValue("@disposition", callDetailRecord.Disposition);
            command.Parameters.AddWithValue("@sourceNum", callDetailRecord.SourceNum);
            command.Parameters.AddWithValue("@destNum", callDetailRecord.DestinationNum);
            command.Parameters.AddWithValue("@callDate", callDetailRecord.CallDate);
            command.Parameters.AddWithValue("@length", callDetailRecord.Length);
            command.Parameters.AddWithValue("@number", callDetailRecord.VoipNumberId);
            command.Parameters.AddWithValue("@incomingOutgoing", callDetailRecord.IncomingOutgoing);
            command.Parameters.AddWithValue("@price_list_id", callDetailRecord.PriceListId);
        }
    }
}
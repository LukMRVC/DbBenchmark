using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using DbBenchmark.ORM.DTO;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class NumberRequestTable
    {
        private static readonly string TableName = "number_request";

        //funkce 9.3
        private static readonly string SQL_SELECT = $"SELECT * FROM {TableName}";

        private static readonly string SQL_SELECT_PRIMARY =
            $"SELECT * FROM {TableName} WHERE participant_id=@participant_id AND number_id=@number_id";

        private static readonly string SQL_SELECT_NUMBER = $"SELECT * FROM {TableName} WHERE number_id=@number_id";

        private static readonly string SQL_SELECT_PARTICIPANT =
            $"SELECT * FROM {TableName} WHERE participant_id=@participant_id";

        //funkce 9.1
        private static readonly string SQL_INSERT =
            $"INSERT INTO {TableName} VALUES (@participant_id, @number_id, CURRENT_TIMESTAMP)";

        //funkce 9.2
        private static readonly string SQL_DELETE_ID =
            $"DELETE FROM {TableName} WHERE participant_id=@participant_id AND number_id=@number_id";

        private static readonly string SQL_UPDATE = $"UPDATE {TableName} SET participant_id=@participant_id," +
                                                    $" number_id=@number_id," +
                                                    $" requested=@requested " +
                                                    $"WHERE participant_id=@participant_id AND number_id=@number_id";

        //funkce 10.3
        private static readonly string SQL_RESOLVE_REQUEST = $"CALL assign_requested_numbers()";


        //funkce 9.1
        public static int Insert(NumberRequest numberRequest, DatabaseConnection connection = null)
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
            PrepareCommand(command, numberRequest);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 10.3
        public static int ResolveRequests(DatabaseConnection connection = null)
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
            var command = db.Command(SQL_RESOLVE_REQUEST);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        public static int Update(NumberRequest numberRequest, DatabaseConnection connection = null)
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
            PrepareCommand(command, numberRequest);
            command.Parameters.AddWithValue("@requested", numberRequest.Requested);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 9.2
        public static int Delete(NumberRequest numberRequest, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@participant_id", numberRequest.Participant.Id);
            command.Parameters.AddWithValue("@number_id", numberRequest.VoipNumber.Id);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 9.3
        public static Collection<NumberRequest> Select(bool relationIgnore = false,
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

            Collection<NumberRequest> numberRequests = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return numberRequests;
        }

        public static NumberRequest SelectPrimary(Participant participant, VoipNumber number,
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

            var command = db.Command(SQL_SELECT_PRIMARY);
            command.Parameters.AddWithValue("@participant_id", participant.Id);
            command.Parameters.AddWithValue("@number_id", number.Id);
            var reader = db.Select(command);
            Collection<NumberRequest> numberRequests = Read(reader, relationIgnore);
            NumberRequest numberRequest = null;
            if (numberRequests.Count > 0)
            {
                numberRequest = numberRequests[0];
            }

            reader.Close();
            if (connection == null)
                db.Close();
            return numberRequest;
        }

        public static Collection<NumberRequest> SelectNumber(VoipNumber number, bool relationIgnore = false,
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
            command.Parameters.AddWithValue("@number_id", number.Id);
            var reader = db.Select(command);
            Collection<NumberRequest> numberRequests = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return numberRequests;
        }

        public static Collection<NumberRequest> SelectParticipant(Participant participant, bool relationIgnore = false,
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

            var command = db.Command(SQL_SELECT_PARTICIPANT);
            command.Parameters.AddWithValue("@participant_id", participant.Id);
            var reader = db.Select(command);
            Collection<NumberRequest> numberRequests = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return numberRequests;
        }

        private static Collection<NumberRequest> Read(NpgsqlDataReader reader, bool relationIgnore)
        {
            Collection<NumberRequest> numberRequests = new Collection<NumberRequest>();
            while (reader.Read())
            {
                NumberRequest numberRequest = new NumberRequest();
                numberRequest.ParticipantId = (int) reader["participant_id"];
                numberRequest.VoipNumberId = (int) reader["number_id"];
                numberRequest.Requested = (DateTime) reader["requested"];
                if (!relationIgnore)
                {
                    numberRequest.Participant = ParticipantTable.Select(numberRequest.ParticipantId, true);
                    numberRequest.VoipNumber = VoipNumberTable.Select(numberRequest.VoipNumberId, true);
                }

                numberRequests.Add(numberRequest);
            }

            return numberRequests;
        }

        public static void PrepareCommand(NpgsqlCommand command, NumberRequest numberRequest)
        {
            command.Parameters.AddWithValue("@participant_id", numberRequest.ParticipantId);
            command.Parameters.AddWithValue("@number_id", numberRequest.VoipNumberId);
        }
    }
}
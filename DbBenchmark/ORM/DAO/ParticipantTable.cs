using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using DbBenchmark.ORM.DTO;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class ParticipantTable
    {
        private static readonly string TableName = "dais.participant";

        //funkce 3.4
        private static readonly string SQL_SELECT = $"SELECT * FROM {TableName} WHERE deleted_at IS NULL";

        //funkce 3.3
        private static readonly string SQL_SELECT_ID =
            $"SELECT * FROM {TableName} WHERE participant_id=@participant_id AND deleted_at IS NULL";

        //funkce 3.1
        private static readonly string SQL_INSERT =
            $"INSERT INTO {TableName} VALUES (@name, @access_level, @contract_id, " +
            $"@password, @balance_limit, CURRENT_TIMESTAMP, NULL)";

        //funkce 3.2
        private static readonly string SQL_DELETE_ID = $"DELETE FROM {TableName} WHERE participant_id=@participant_id";

        //funkce 10.4
        private static readonly string SQL_CALLS_COST =
            $"SELECT dais.calcParticipantCallCost(@participant_id, @start, @end) AS calls_cost";


        //funkce 3.4
        private static readonly string SQL_SELECT_CONTRACT =
            $"SELECT * FROM {TableName} WHERE contract_id=@contract_id AND deleted_at IS NULL";

        //funkce 3.7 - Aktualizace účastníka, tuto funkci jsem zapomněl napsat do specifikace 
        private static readonly string SQL_UPDATE =
            $"UPDATE {TableName} SET name=@name, access_level=@access_level, contract_id=@contract_id, " +
            $"password=@password, balance_limit=@balance_limit WHERE participant_id=@participant_id";

        private static readonly string SQL_SELECT_LAST_ID = $"SELECT MAX(participant_id) FROM {TableName}";

        //funkce 3.1
        public static int Insert(Participant participant, DatabaseConnection connection = null)
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
            PrepareCommand(command, participant);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            if (ret > 0)
            {
                participant.Id = SelectLastId(db);
            }

            return ret;
        }

        private static int SelectLastId(DatabaseConnection connection = null)
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
            int lastId;
            var command = db.Command(SQL_SELECT_LAST_ID);
            using (var reader = db.Select(command))
            {
                reader.Read();
                lastId = reader.GetInt32(0);
            }

            if (connection == null)
                db.Close();
            return lastId;
        }

        //funkce 3.7 - Aktualizace účastníka, tuto funkci jsem zapomněl napsat do specifikace 
        public static int Update(Participant participant, DatabaseConnection connection = null)
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
            PrepareCommand(command, participant);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 3.2
        public static int Delete(Participant participant, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@participant_id", participant.Id);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 3.4
        public static Collection<Participant> Select(bool relationIgnore = false, DatabaseConnection connection = null)
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

            Collection<Participant> participants = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return participants;
        }

        // funkce 3.5
        public static int RequestNumber(Participant participant, VoipNumber voipNumber,
            DatabaseConnection connection = null)
        {
            if (!voipNumber.CanBeRequest)
                throw new Exception("This number cannot be requested");
            return NumberRequestTable.Insert(new NumberRequest {Participant = participant, VoipNumber = voipNumber},
                connection);
        }

        //funkce 3.6
        public static int GiveNumber(Participant giver, VoipNumber voipNumber, Participant receiver,
            DatabaseConnection connection = null)
        {
            if (!giver.CanGive(voipNumber))
                throw new Exception("This number cannot be requested");
            voipNumber.Participant = receiver;
            return VoipNumberTable.Update(voipNumber);
        }

        //funkce 10.4
        public static double CalcCallsCost(Participant participant, DateTime start, DateTime end,
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
            if (start > end)
                throw new Exception("Start date cannot be greater than end date");

            var command = db.Command(SQL_CALLS_COST);
            command.Parameters.AddWithValue("@participant_id", participant.Id);
            command.Parameters.AddWithValue("@start", start);
            command.Parameters.AddWithValue("@end", end);
            double num = 0;
            using (var reader = db.Select(command))
            {
                if (reader.Read())
                {
                    num = reader.GetDouble(0);
                }
            }

            if (connection == null)
                db.Close();
            return num;
        }

        //funkce 3.4
        public static Collection<Participant> SelectForContract(Contract contract, DatabaseConnection connection = null)
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

            Collection<Participant> participants = Read(reader, true);
            reader.Close();
            if (connection == null)
                db.Close();
            return participants;
        }

        //funkce 3.3
        public static Participant Select(int id, bool relationIgnore = false, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@participant_id", id);
            var reader = db.Select(command);
            Collection<Participant> participants = Read(reader, relationIgnore);
            Participant address = null;
            if (participants.Count > 0)
            {
                address = participants[0];
            }

            reader.Close();
            if (connection == null)
                db.Close();
            return address;
        }

        private static Collection<Participant> Read(NpgsqlDataReader reader, bool relationIgnore)
        {
            Collection<Participant> participants = new Collection<Participant>();
            while (reader.Read())
            {
                Participant participant = new Participant();
                participant.Id = (int) reader["participant_id"];
                participant.Name = (string) reader["name"];
                participant.AccessLevel = (byte) reader["access_level"];
                participant.ContractId = (int) reader["contract_id"];
                if (!relationIgnore)
                {
                    participant.Contract = ContractTable.Select(participant.ContractId, true);
                    participant.VoipNumbers = VoipNumberTable.SelectParticipant(participant, true);
                    participant.NumberRequests = NumberRequestTable.SelectParticipant(participant, true);
                }

                participant.Password = (string) reader["password"];
                if (!reader.IsDBNull("balance_limit"))
                {
                    participant.BalanceLimit = Decimal.ToDouble((Decimal) reader["balance_limit"]);
                }

                participant.CreatedAt = (DateTime) reader["created_at"];
                if (!reader.IsDBNull("deleted_at"))
                {
                    participant.DeletedAt = (DateTime) reader["deleted_at"];
                }

                participants.Add(participant);
            }

            return participants;
        }

        public static void PrepareCommand(NpgsqlCommand command, Participant participant)
        {
            command.Parameters.AddWithValue("@participant_id", participant.Id);
            command.Parameters.AddWithValue("@name", participant.Name);
            command.Parameters.AddWithValue("@access_level", participant.AccessLevel);
            command.Parameters.AddWithValue("@contract_id", participant.ContractId);
            command.Parameters.AddWithValue("@password", participant.Password);
            command.Parameters.AddWithValue("@balance_limit", (object) participant.BalanceLimit ?? DBNull.Value);
        }
    }
}
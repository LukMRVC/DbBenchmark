using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using DbBenchmark.ORM.DTO;
using Npgsql;

namespace DbBenchmark.ORM.DAO
{
    public class VoipNumberTable
    {
        private static readonly string TableName = "dais.voip_number";

        //funkce 4.4
        private static readonly string SQL_SELECT = $"SELECT * FROM {TableName} WHERE deleted_at IS NULL";

        private static readonly string SQL_SELECT_ID =
            $"SELECT * FROM {TableName} WHERE number_id=@number_id AND deleted_at IS NULL";

        private static readonly string SQL_SELECT_PARTICIPANT =
            $"SELECT * FROM {TableName} WHERE participant_id=@participant_id AND deleted_at IS NULL";

        //funkce 4.1
        private static readonly string SQL_INSERT =
            $"INSERT INTO {TableName} VALUES (@pcc, @number, NULL, @password, @current_state, @foreign_block, NULL, NULL, NULL)";

        //funkce 4.3
        private static readonly string SQL_DELETE_ID = $"DELETE FROM {TableName} WHERE number_id=@number_id";

        //funkce 4.2
        private static readonly string SQL_UPDATE =
            $"UPDATE {TableName} SET phone_country_code=@pcc, number=@number, participant_id=@participant_id," +
            $" current_state=@current_state, foreign_block=@foreign_block, quarantine_until=@quarantine_until, " +
            $" activated=@activated WHERE number_id=@number_id";

        private static readonly string SQL_LAST_ID = $"SELECT MAX(number_id) FROM {TableName}";

        public static void PrepareCommand(NpgsqlCommand command, VoipNumber voipNumber)
        {
            command.Parameters.AddWithValue("@number_id", voipNumber.Id);
            command.Parameters.AddWithValue("@pcc", voipNumber.PhoneCountryCode);
            command.Parameters.AddWithValue("@number", voipNumber.Number);
            command.Parameters.AddWithValue("@current_state", voipNumber.CurrentState);
            command.Parameters.AddWithValue("@foreign_block", voipNumber.ForeignBlock);
            command.Parameters.AddWithValue("@quarantine_until", (object) voipNumber.QuarantineUntil ?? DBNull.Value);
            command.Parameters.AddWithValue("@activated", (object) voipNumber.Activated ?? DBNull.Value);
            command.Parameters.AddWithValue("@password", voipNumber.Password);
            command.Parameters.AddWithValue("@participant_id",
                voipNumber.ParticipantId.HasValue ? (object) voipNumber.ParticipantId.Value : DBNull.Value);
        }


        //funkce 4.1
        public static int Insert(VoipNumber voipNumber, DatabaseConnection connection = null)
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
            PrepareCommand(command, voipNumber);
            int ret = db.Execute(command);
            if (ret > 0)
            {
                voipNumber.Id = SelectLastId(db);
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
            using (NpgsqlDataReader reader = db.Select(command))
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

        //funkce 4.5
        public static int NumberRemoval(VoipNumber voipNumber, DatabaseConnection connection = null)
        {
            voipNumber.Participant = null;
            voipNumber.ParticipantId = null;
            return Update(voipNumber, connection);
        }

        //funkce 4.2
        public static int Update(VoipNumber voipNumber, DatabaseConnection connection = null)
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
            PrepareCommand(command, voipNumber);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 4.3
        public static int Delete(VoipNumber voipNumber, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@number_id", voipNumber.Id);
            int ret = db.Execute(command);
            if (connection == null)
                db.Close();
            return ret;
        }

        //funkce 4.4
        public static Collection<VoipNumber> Select(bool relationIgnore = false, DatabaseConnection connection = null)
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

            Collection<VoipNumber> voipNumbers = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return voipNumbers;
        }

        public static Collection<VoipNumber> SelectParticipant(Participant participant, bool relationIgnore = false,
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
            Collection<VoipNumber> voipNumbers = Read(reader, relationIgnore);
            reader.Close();
            if (connection == null)
                db.Close();
            return voipNumbers;
        }

        public static VoipNumber Select(int id, bool relationIgnore = false, DatabaseConnection connection = null)
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
            command.Parameters.AddWithValue("@number_id", id);
            var reader = db.Select(command);
            Collection<VoipNumber> voipNumbers = Read(reader, relationIgnore);
            VoipNumber voipNumber = null;
            if (voipNumbers.Count > 0)
            {
                voipNumber = voipNumbers[0];
            }

            reader.Close();
            if (connection == null)
                db.Close();
            return voipNumber;
        }

        private static Collection<VoipNumber> Read(NpgsqlDataReader reader, bool relationIgnore)
        {
            Collection<VoipNumber> voipNumbers = new Collection<VoipNumber>();
            while (reader.Read())
            {
                VoipNumber voipNumber = new VoipNumber();
                voipNumber.Id = (int) reader["number_id"];
                voipNumber.PhoneCountryCode = (int) reader["phone_country_code"];
                voipNumber.Number = (int) reader["number"];
                if (!reader.IsDBNull("participant_id"))
                    voipNumber.ParticipantId = (int) reader["participant_id"];
                if (!relationIgnore)
                {
                    if (voipNumber.ParticipantId.HasValue)
                    {
                        voipNumber.Participant = ParticipantTable.Select(voipNumber.ParticipantId.Value, true);
                    }

                    voipNumber.NumberRequest = NumberRequestTable.SelectNumber(voipNumber, true);
                    voipNumber.CallDetailRecords = CallDetailRecordTable.SelectForNumber(voipNumber, true);
                }

                voipNumber.Password = (string) reader["password"];
                voipNumber.CurrentState = (byte) reader["current_state"];
                voipNumber.ForeignBlock = (bool) reader["foreign_block"];
                if (!reader.IsDBNull("quarantine_until"))
                    voipNumber.QuarantineUntil = (DateTime) reader["quarantine_until"];
                if (!reader.IsDBNull("activated"))
                    voipNumber.Activated = (DateTime) reader["activated"];
                if (!reader.IsDBNull("deleted_at"))
                    voipNumber.DeletedAt = (DateTime) reader["deleted_at"];
                voipNumbers.Add(voipNumber);
            }

            return voipNumbers;
        }
    }
}
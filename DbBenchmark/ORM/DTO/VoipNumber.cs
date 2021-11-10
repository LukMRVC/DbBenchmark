using System;
using System.Collections.Generic;
using System.Text;
using DbBenchmark.ORM.DAO;

namespace DbBenchmark.ORM.DTO
{
    public class VoipNumber
    {
        public int Id { get; set; }
        public int PhoneCountryCode { get; set; }
        public int Number { get; set; }
        private string _password { get; set; }

        public string Password
        {
            get => _password;
            set => _password = CreateMD5(value);
        }

        public int CurrentState { get; set; }
        public bool ForeignBlock { get; set; }
        public DateTime? QuarantineUntil { get; set; }
        public DateTime? Activated { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? ParticipantId { get; set; }
        private Participant _participant;

        public Participant Participant
        {
            get => _participant;
            set
            {
                _participant = value;
                ParticipantId = value.Id;
            }
        }

        public IEnumerable<NumberRequest> NumberRequest { get; set; }
        public IEnumerable<CallDetailRecord> CallDetailRecords;

        public bool CanBeRequest
        {
            get
            {
                if (QuarantineUntil == null && CurrentState == 0 && ParticipantId == null) return true;
                return false;
            }
        }

        private static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public string GetState()
        {
            if (CurrentState == 0 && NumberRequestTable.SelectNumber(this, true).Count <= 0
                                  && !QuarantineUntil.HasValue && !Activated.HasValue)
                return "Neaktivní";
            else if (CurrentState == 0 && NumberRequestTable.SelectNumber(this, true).Count > 0
                                       && !QuarantineUntil.HasValue && !Activated.HasValue)
                return "Zažádané";
            else if (CurrentState == 1 && !QuarantineUntil.HasValue && Activated.HasValue)
                return "Aktivní";
            else if (CurrentState == 0 && !QuarantineUntil.HasValue && Activated.HasValue)
                return "Blokované";
            else if (CurrentState == 0 && QuarantineUntil.HasValue && !Activated.HasValue)
                return "V karanténě";
            else
                return "Neznámý stav";
        }
    }
}
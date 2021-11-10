using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DbBenchmark.ORM.DAO;

namespace DbBenchmark.ORM.DTO
{
    public class Participant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccessLevel { get; set; }
        private string _password { get; set; }

        public string Password
        {
            get => _password;
            set => _password = Hash(value);
        }

        public double? BalanceLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        public int ContractId { get; set; }
        public IEnumerable<VoipNumber> VoipNumbers { get; set; } = new List<VoipNumber>();
        public IEnumerable<NumberRequest> NumberRequests { get; set; } = new List<NumberRequest>();

        private Contract _contract { get; set; }

        public Contract Contract
        {
            get => _contract;
            set
            {
                _contract = value;
                ContractId = value.Id;
            }
        }

        private static string Hash(string pass)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(pass));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        //handover number that I have to another participant
        //function 3.6
        public int Handover(Participant participant, VoipNumber number)
        {
            if (VoipNumbers.Contains(number))
            {
                number.ParticipantId = participant.Id;
                number.Participant = participant;
                return VoipNumberTable.Update(number);
            }

            return 0;
        }

        public bool CanGive(VoipNumber number)
        {
            return Id == number.ParticipantId;
        }
    }
}
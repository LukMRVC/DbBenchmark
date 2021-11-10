using System;

namespace DbBenchmark.ORM.DTO
{
    public class NumberRequest
    {
        public int ParticipantId { get; set; }
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

        public int VoipNumberId { get; set; }

        private VoipNumber _voipNumber;

        public VoipNumber VoipNumber
        {
            get => _voipNumber;
            set
            {
                _voipNumber = value;
                VoipNumberId = value.Id;
            }
        }

        public DateTime Requested { get; set; }
    }
}
using System;

namespace DbBenchmark.ORM.DTO
{
    public class CallDetailRecord
    {
        public long Id { get; set; }
        public string Disposition { get; set; }
        public string SourceNum { get; set; }
        public string DestinationNum { get; set; }
        public int Length { get; set; }
        public DateTime CallDate { get; set; }
        public bool IncomingOutgoing { get; set; }

        public int VoipNumberId { get; set; }
        public VoipNumber VoipNumber { get; set; }
        public PriceList PriceList { get; set; }
        public int PriceListId { get; set; }
    }
}
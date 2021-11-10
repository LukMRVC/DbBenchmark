using System.Collections.Generic;

namespace DbBenchmark.ORM.DTO
{
    public class PriceList
    {
        public int Id { get; set; }
        public int TarificationFirst { get; set; }
        public int TarificationSecond { get; set; }
        public int PricePerSecond { get; set; }
        public int PhoneCountryCode { get; set; }

        public IEnumerable<CallDetailRecord> CallDetailRecord { get; set; }
    }
}
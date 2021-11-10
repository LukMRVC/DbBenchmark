using System;
using System.Collections.Generic;

namespace DbBenchmark.ORM.DTO
{
    public class Invoice
    {
        public int Number { get; set; }
        public double Amount { get; set; }
        public int TaxValuePercent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime TaxablePeriod { get; set; }
        public DateTime Maturity { get; set; }
        public DateTime? Paid { get; set; }
        public int ContractId { get; set; }

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

        public IEnumerable<InvoiceHasItem> Items { get; set; } = new List<InvoiceHasItem>();
    }
}
using System.Collections.Generic;

namespace DbBenchmark.ORM.DTO
{
    public class InvoiceItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double UnitCost { get; set; }
        public IEnumerable<InvoiceHasItem> Invoices { get; set; }
    }
}
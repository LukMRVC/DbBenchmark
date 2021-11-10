namespace DbBenchmark.ORM.DTO
{
    public class InvoiceHasItem
    {
        public int InvoiceNumber { get; set; }

        private Invoice _invoice;

        public Invoice Invoice
        {
            get => _invoice;
            set
            {
                _invoice = value;
                InvoiceNumber = value.Number;
            }
        }

        public int InvoiceItemId { get; set; }
        private InvoiceItem _invoiceItem;

        public InvoiceItem InvoiceItem
        {
            get => _invoiceItem;
            set
            {
                _invoiceItem = value;
                InvoiceItemId = value.Id;
            }
        }

        public double ItemUnitCost { get; set; }
        public int ItemCount { get; set; }
    }
}
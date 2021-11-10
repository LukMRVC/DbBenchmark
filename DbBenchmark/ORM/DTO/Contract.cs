using System;
using System.Collections.Generic;

namespace DbBenchmark.ORM.DTO
{
    public class Contract
    {
        public int Id { get; set; }
        public int VariableSymbol { get; set; }
        public string ContractName { get; set; }
        public long? IdentificationNumber { get; set; }
        public string? VatIdentificationNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public double? NotifyLimit { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public double? BonusAmount { get; set; }

        public Address Address { get; set; }
        public IEnumerable<Participant> Participants { get; set; }
        public IEnumerable<Invoice> Invoices { get; set; }

        public override string ToString()
        {
            return $"{ContractName}, {VariableSymbol}";
        }
    }
}
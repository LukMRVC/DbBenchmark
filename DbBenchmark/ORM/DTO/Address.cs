namespace DbBenchmark.ORM.DTO
{
    public class Address
    {
        public int Id { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string StreetName { get; set; }
        public int HouseNumber { get; set; }
        public int ZipCode { get; set; }

        private Contract _contract;

        public Contract Contract
        {
            get => _contract;
            set
            {
                _contract = value;
                ContractId = value.Id;
            }
        }

        public int? ContractId { get; set; }

        public string FullAddress => $"{City} {District}, {StreetName} {HouseNumber}, {ZipCode}";

        public bool CanBeDeleted
        {
            get
            {
                if (Contract != null && ContractId != null && Contract.DeletedAt.HasValue) return true;
                return false;
            }
        }
    }
}
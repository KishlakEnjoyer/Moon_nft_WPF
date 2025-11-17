namespace Moon_nft_api.DTOs
{
    public class transactionDTO
    {
        public int IdTransaction { get; set; }

        public int IdSaler { get; set; }
        public string NameSaler { get; set; }
        public int IdBuyer { get; set; }
        public string NameBuyer { get; set; }
        public int IdPresent { get; set; }
        public string CollectionPresent { get; set; }
        public byte[] ImagePresent { get; set; }
        public string displayNum { get; set; }
        public DateOnly DateTransaction { get; set; }
        public float SumTransaction { get; set; }
    }
}

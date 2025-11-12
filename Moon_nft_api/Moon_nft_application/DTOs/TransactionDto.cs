namespace Moon_nft_api.Models
{
    public class TransactionDto
    {
        public int IdTransaction { get; set; }
        public int IdSaler { get; set; }
        public int IdBuyer { get; set; }
        public int IdPresent { get; set; }
        public DateOnly DateTransaction { get; set; }
        public float SumTransaction { get; set; }

        public string BuyerName { get; set; }
        public string SalerName { get; set; }
        public string PresentName { get; set; }
    }
}

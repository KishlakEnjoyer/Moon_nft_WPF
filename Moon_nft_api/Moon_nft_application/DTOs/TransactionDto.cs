namespace Moon_nft_api.DTOs
{
    public class transactionDTO
    {
        public int IdTransaction { get; set; }

        public int IdSaler { get; set; }

        public int IdBuyer { get; set; }

        public int IdPresent { get; set; }

        public DateOnly DateTransaction { get; set; }

        public float SumTransaction { get; set; }
    }
}

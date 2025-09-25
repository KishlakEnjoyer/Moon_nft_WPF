namespace Moon_nft_api.Models
{
    public class TopUpRequest
    {
        public string Email { get; set; } = string.Empty;
        public float Amount { get; set; }
    }
}

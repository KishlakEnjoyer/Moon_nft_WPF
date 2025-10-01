namespace Moon_nft_api.Models
{
    public class ProfileResponseDto
    {
        public string Nickname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public float? Balance { get; set; }
    }
}

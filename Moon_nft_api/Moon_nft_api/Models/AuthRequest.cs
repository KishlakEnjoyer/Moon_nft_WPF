namespace Moon_nft_api.Models
{
    public class RegisterRequest
    {
        public long TgId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthResponse
    {
        public long TgId { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }
}

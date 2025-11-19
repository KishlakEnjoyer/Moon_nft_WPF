namespace Moon_nft_api.DTOs
{
    public class UpdateProfileRequest
    {
        public int UserId { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewNickname { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }
}

namespace Moon_NFT_WPFAPI.EmailModels
{
    public class VerifyCodeRequest
    {
        public string TempId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}

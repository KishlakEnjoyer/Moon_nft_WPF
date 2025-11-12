namespace Moon_nft_api.DTOs
{
    public class UserProfileDto
    {
        public int IdUser { get; set; }
        public string NicknameUser { get; set; }
        public string EmailUser { get; set; }
        public string RoleUser { get; set; }
        public DateOnly DateRegUser { get; set; }
        public float BalanceUser { get; set; }
        public int RatingUser { get; set; }

        public List<TransactionDto> TransactionIds { get; set; }

        public List<PresentDto> OwnedPresentIds { get; set; }

        public List<LotDto> CreatedLotIds { get; set; }
    }
}

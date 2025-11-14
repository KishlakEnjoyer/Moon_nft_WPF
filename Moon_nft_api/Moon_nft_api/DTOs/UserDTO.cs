namespace Moon_nft_api.DTOs
{
    public class UserDTO
    {
        public int IdUser { get; set; }

        public long? IdTgUser { get; set; }

        public string? EmailUser { get; set; }

        public string? PasswordUser { get; set; }

        public string? NicknameUser { get; set; }

        public DateOnly? DateRegUser { get; set; }

        public string? RoleUser { get; set; }

        public float? RatingUser { get; set; }

        public float? BalanceUser { get; set; }

        public List<presentDTO> PresentsUser { get; set; }
        public List<LotDTO> CartUser {  get; set; }
        public List<LotDTO> LotsUser { get; set; }
        public List<transactionDTO> TransactionUser { get; set; }
    }
}

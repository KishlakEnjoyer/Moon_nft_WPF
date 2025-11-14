namespace Moon_NFT_WPFAPI.DTOs
{
    public class presentDTO
    {
        public int IdPresent {  get; set; }

        public int OwneridPresent { get; set; }
        public string OwnernamePresent { get; set; } = string.Empty;


        public int IdPresentCollection { get; set; }
        public string CollectionName { get; set; } = string.Empty;

        public int? IdModel { get; set; }
        public string? ModelName { get; set; } = string.Empty;

        public int? IdBackground { get; set; }
        public string? BackgroundName { get; set; } = string.Empty;

        public int? IdSymbol { get; set; }
        public string? SymbolName { get; set; } = string.Empty;

        public string displayNum {  get; set; } = string.Empty;

        public byte[] ImagePresent {  get; set; }


        public sbyte? UpgradePresent { get; set; }

        public DateOnly? DateUpgradePresent { get; set; }


        public float? priceLotPresent { get; set; }

    }
}

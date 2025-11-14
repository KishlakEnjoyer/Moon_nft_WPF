namespace Moon_NFT_WPFAPI.DTOs
{
    public class LotDTO
    {
        public int IdLot { get; set; }

        public int IdPresent { get; set; }

        public DateOnly? DateUpgradePresent { get; set; }

        public int _collectionId { get; set; }
        public string _collectionName { get; set; }

        public int _modelId { get; set; }
        public string _modelName { get; set; }

        public int _bgId { get; set; }
        public string _bgName { get; set; }

        public int _symbolId { get; set; }
        public string _symbolName { get; set; }

        public int IdSaler { get; set; }
        public string SalerNickname { get; set; }

        public float? PriceLot { get; set; }

        public string? statusLot {  get; set; }

    }
}

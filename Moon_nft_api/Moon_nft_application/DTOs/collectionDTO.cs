namespace Moon_nft_api.DTOs
{
    public class collectionDTO
    {
        public int IdPresentCollections {  get; set; }
        public string NamePresentCollection { get; set; } = string.Empty;
        public byte[]? ImagePresentcollections { get; set; }
        public int? LimitPresentCollection { get; set; }
        public int? AvailableCount { get; set; }
        public float? PricePresentCollection { get; set; }

    }
}

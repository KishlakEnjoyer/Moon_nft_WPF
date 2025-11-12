using Moon_nft_api.DTOs;

namespace Moon_nft_api.Models
{
    public class PresentcollectionDto
    {
        public int IdPresentCollections { get; set; }
        public string NamePresentCollection { get; set; } = "";
        public decimal PricePresentCollection { get; set; }
        public int? AvailableCount { get; set; }
        public int? LimitPresentCollection { get; set; }
        public byte[]? ImagePresentcollections { get; set; }
        public List<ModelDto>? IdModels { get; set; }
    }
}

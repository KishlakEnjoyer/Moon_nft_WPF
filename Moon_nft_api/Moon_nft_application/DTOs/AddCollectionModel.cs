namespace Moon_nft_api.Services
{
    // Модель для данных
    public class AddCollectionModel
    {
        public string Name { get; set; }
        public string ImageBase64 { get; set; } 
        public int Limit { get; set; }
        public int Count { get; set; }
        public float Price { get; set; }
    }
}

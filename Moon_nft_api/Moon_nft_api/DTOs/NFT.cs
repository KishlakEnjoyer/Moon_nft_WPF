namespace Moon_nft_api.DTOs
{
    public class NFT
    {
        public string modelPath { get; set; }
        public string symbolPath { get; set; }
        public string bgHEX { get; set; }

        public NFT(string modelPath, string symbolPath, string bgHEX)
        {
            this.modelPath = modelPath;
            this.symbolPath = symbolPath;
            this.bgHEX = bgHEX;
        }
    }
}

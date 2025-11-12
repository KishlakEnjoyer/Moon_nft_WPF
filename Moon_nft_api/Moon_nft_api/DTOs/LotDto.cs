using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moon_nft_api.DTOs
{
    public class LotDto
    {
        public int IdLot { get; set; }
        public int IdPresent { get; set; }
        public int IdSaler { get; set; }
        public float PriceLot { get; set; }
        public string StatusLot { get; set; }
        public string PresentName { get; set; }
        public string? ModelName { get; set; }
        public string? BackgroundName { get; set; }
        public string? SymbolName { get; set; }
        public byte[]? ImagePresent { get; set; }
        public DateOnly? DateUpgradePresent { get; set; }
        public int NumPresent { get; set; }
        public int? PresentCollectionLimit { get; set; }
        public string DisplayNum => $"#{NumPresent} / {PresentCollectionLimit ?? 0}";
    }
}

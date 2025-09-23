using System;
using System.Collections.Generic;

namespace Moon_nft_api.Models;

public partial class Symbol
{
    public int IdSymbol { get; set; }

    public string? NameSymbol { get; set; }

    public byte[]? ImageSymbol { get; set; }

    public virtual ICollection<Present> Presents { get; set; } = new List<Present>();
}

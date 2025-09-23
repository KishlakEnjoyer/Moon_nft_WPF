using System;
using System.Collections.Generic;

namespace Moon_nft_api.Models;

public partial class Presentcollection
{
    public int IdPresentCollections { get; set; }

    public string? NamePresentCollection { get; set; }

    public byte[]? ImagePresentcollections { get; set; }

    public int? LimitPresentCollection { get; set; }

    public int? AvailableCount { get; set; }

    public virtual ICollection<Present> Presents { get; set; } = new List<Present>();
}

using System;
using System.Collections.Generic;

namespace Moon_nft_api.Models;

public partial class Model
{
    public int IdModel { get; set; }

    public string? NameModel { get; set; }

    public byte[]? ImageModel { get; set; }

    public virtual ICollection<Present> Presents { get; set; } = new List<Present>();

    public virtual ICollection<Presentcollection> IdCollections { get; set; } = new List<Presentcollection>();
}

using System;
using System.Collections.Generic;

namespace Moon_nft_api.Models;

public partial class Background
{
    public int IdBackground { get; set; }

    public string? NameBackground { get; set; }

    public string? ColorBackground { get; set; }

    public virtual ICollection<Present> Presents { get; set; } = new List<Present>();
}

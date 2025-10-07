using System;
using System.Collections.Generic;

namespace Moon_nft_application.Models;

public partial class Lot
{
    public int IdLot { get; set; }

    public int IdPresent { get; set; }

    public int IdSaler { get; set; }

    public float? PriceLot { get; set; }

    public string? StatusLot { get; set; }

    public virtual Present IdPresentNavigation { get; set; } = null!;

    public virtual ICollection<User> IdTgUsers { get; set; } = new List<User>();
}

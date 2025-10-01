using System;
using System.Collections.Generic;

namespace Moon_nft_api.Models;

public partial class Transaction
{
    public int IdTransaction { get; set; }

    public int IdSaler { get; set; }

    public int IdBuyer { get; set; }

    public int IdPresent { get; set; }

    public DateOnly DateTransaction { get; set; }

    public float SumTransaction { get; set; }

    public virtual Present IdPresentNavigation { get; set; } = null!;
}

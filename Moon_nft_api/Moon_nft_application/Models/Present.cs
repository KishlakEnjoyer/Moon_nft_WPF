using System;
using System.Collections.Generic;

namespace Moon_nft_application.Models;

public partial class Present
{
    public int IdPresent { get; set; }

    public int AuthoridPresent { get; set; }

    public int OwneridPresent { get; set; }

    public int IdPresentCollection { get; set; }

    public int IdModel { get; set; }

    public int IdBackground { get; set; }

    public int IdSymbol { get; set; }

    public int NumPresent { get; set; }

    public byte[]? ImagePresent { get; set; }

    public string? DescPresent { get; set; }

    public sbyte? UpgradePresent { get; set; }

    public DateOnly? DateUpgradePresent { get; set; }

    public virtual Background IdBackgroundNavigation { get; set; } = null!;

    public virtual Model IdModelNavigation { get; set; } = null!;

    public virtual Presentcollection IdPresentCollectionNavigation { get; set; } = null!;

    public virtual Symbol IdSymbolNavigation { get; set; } = null!;

    public virtual Lot? Lot { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

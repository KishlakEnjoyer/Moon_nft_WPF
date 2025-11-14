using System;
using System.Collections.Generic;

namespace Moon_NFT_WPFAPI.Models;

public partial class Present
{
    public int IdPresent { get; set; }

    public int AuthoridPresent { get; set; }

    public int OwneridPresent { get; set; }

    public int IdPresentCollection { get; set; }

    public int? IdModel { get; set; }

    public int? IdBackground { get; set; }

    public int? IdSymbol { get; set; }

    public int NumPresent { get; set; }

    public byte[]? ImagePresent { get; set; }

    public string? DescPresent { get; set; }

    public sbyte? UpgradePresent { get; set; }

    public DateOnly? DateUpgradePresent { get; set; }

    public virtual User AuthoridPresentNavigation { get; set; } = null!;

    public virtual Background? IdBackgroundNavigation { get; set; }

    public virtual Model? IdModelNavigation { get; set; }

    public virtual Presentcollection IdPresentCollectionNavigation { get; set; } = null!;

    public virtual Symbol? IdSymbolNavigation { get; set; }

    public virtual Lot? Lot { get; set; }

    public virtual User OwneridPresentNavigation { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public string displayNum => $"#{NumPresent} / {IdPresentCollectionNavigation.LimitPresentCollection}";
}

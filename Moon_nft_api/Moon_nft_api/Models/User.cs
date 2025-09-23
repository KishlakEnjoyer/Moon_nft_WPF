using System;
using System.Collections.Generic;

namespace Moon_nft_api.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string? EmailUser { get; set; }

    public string? PasswordUser { get; set; }

    public string? NicknameUser { get; set; }

    public DateOnly? DateRegUser { get; set; }

    public string? RoleUser { get; set; }

    public float? RatingUser { get; set; }

    public virtual ICollection<Present> PresentAuthoridPresentNavigations { get; set; } = new List<Present>();

    public virtual ICollection<Present> PresentOwneridPresentNavigations { get; set; } = new List<Present>();

    public virtual ICollection<Transaction> TransactionIdBuyerNavigations { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionIdSalerNavigations { get; set; } = new List<Transaction>();
}

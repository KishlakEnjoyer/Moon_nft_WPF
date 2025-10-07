using System;
using System.Collections.Generic;

namespace Moon_nft_application.Models;

public partial class User
{
    public long IdTgUser { get; set; }

    public string? EmailUser { get; set; }

    public string? PasswordUser { get; set; }

    public string? NicknameUser { get; set; }

    public DateOnly? DateRegUser { get; set; }

    public string? RoleUser { get; set; }

    public float? RatingUser { get; set; }

    public float? BalanceUser { get; set; }

    public virtual ICollection<Transaction> TransactionIdBuyerNavigations { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionIdSalerNavigations { get; set; } = new List<Transaction>();

    public virtual ICollection<Lot> IdPresents { get; set; } = new List<Lot>();
}

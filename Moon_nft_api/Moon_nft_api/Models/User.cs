using System;
using System.Collections.Generic;

namespace Moon_nft_api.Models;

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
}

using Microsoft.AspNetCore.Identity;
using OrchardCore.Entities;

namespace OrchardCore.Users.Models;

public class User : Entity, IUser
{
    public long Id { get; set; }

    public string UserId { get; set; }

    public string UserName { get; set; }

    public string NormalizedUserName { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string NormalizedEmail { get; set; }

    public string PasswordHash { get; set; }

    public string SecurityStamp { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool TwoFactorEnabled { get; set; }

    public bool IsLockoutEnabled { get; set; }

    public DateTime? LockoutEndUtc { get; set; }

    public int AccessFailedCount { get; set; }

    public string ResetToken { get; set; }

    public IList<string> RoleNames { get; set; } = [];

    public IList<UserClaim> UserClaims { get; set; } = [];

    public IList<UserLoginInfo> LoginInfos { get; set; } = [];

    public IList<UserToken> UserTokens { get; set; } = [];

    public override string ToString()
    {
        return UserName;
    }
}

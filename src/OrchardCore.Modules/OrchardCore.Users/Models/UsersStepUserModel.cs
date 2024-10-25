namespace OrchardCore.Users.Models;

public class UsersStepUserModel
{
    public long Id { get; set; }

    public string UserId { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool IsEnabled { get; set; } = true;

    public string NormalizedEmail { get; set; }

    public string NormalizedUserName { get; set; }

    public string SecurityStamp { get; set; }

    public string ResetToken { get; set; }

    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public bool IsLockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public IList<string> RoleNames { get; set; }
}

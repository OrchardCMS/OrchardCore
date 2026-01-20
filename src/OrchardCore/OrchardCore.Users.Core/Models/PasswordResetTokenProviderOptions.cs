using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.Models;

public sealed class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public PasswordResetTokenProviderOptions()
    {
        Name = "PasswordResetDataProtectorTokenProvider";
        TokenLifespan = TimeSpan.FromMinutes(15);
    }
}

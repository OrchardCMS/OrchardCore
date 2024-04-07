using System;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.Models;

public sealed class TwoFactorEmailTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public TwoFactorEmailTokenProviderOptions()
    {
        Name = "TwoFactorEmailDataProtectorTokenProvider";
        TokenLifespan = TimeSpan.FromMinutes(5);
    }
}

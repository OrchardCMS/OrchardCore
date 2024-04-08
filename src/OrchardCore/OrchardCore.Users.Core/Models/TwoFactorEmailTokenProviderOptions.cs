using System;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Models;

public sealed class TwoFactorEmailTokenProviderOptions
{
    //
    // Summary:
    //     Gets or sets the amount of time a generated token remains valid. Defaults to
    //     5 Minutes.
    //
    // Value:
    //     The amount of time a generated token remains valid.
    public TimeSpan TokenLifespan { get; set; }

    public Rfc6238TokenLength TokenLength { get; set; }

    public TwoFactorEmailTokenProviderOptions()
    {
        TokenLifespan = TimeSpan.FromMinutes(5);
        TokenLength = Rfc6238TokenLength.Eight;
    }
}

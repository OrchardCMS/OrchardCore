using System;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Models;

public sealed class TwoFactorEmailTokenProviderOptions
{
    /// <summary>
    /// Gets or sets the amount of time a generated token remains valid.
    /// The amount of time a generated token remains valid. Default value is 5 minutes.
    /// </summary>
    public TimeSpan TokenLifespan { get; set; }

    /// <summary>
    /// Gets or sets the length of the generated token. Default value is 8 digits long.
    /// </summary>
    public Rfc6238TokenLength TokenLength { get; set; }

    public TwoFactorEmailTokenProviderOptions()
    {
        TokenLifespan = TimeSpan.FromMinutes(5);
        TokenLength = Rfc6238TokenLength.Eight;
    }
}

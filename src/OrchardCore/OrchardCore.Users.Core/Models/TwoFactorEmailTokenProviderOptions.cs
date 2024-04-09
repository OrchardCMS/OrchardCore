using System;

namespace OrchardCore.Users.Models;

public sealed class TwoFactorEmailTokenProviderOptions
{
    /// <summary>
    /// Gets or sets the amount of time a generated token remains valid.
    /// The amount of time a generated token remains valid. Default value is 5 minutes.
    /// </summary>
    public TimeSpan TokenLifespan { get; set; }

    /// <summary>
    /// Gets or sets the generated token's length. Default value is 8 digits long.
    /// </summary>
    public TwoFactorEmailTokenLength TokenLength { get; set; }

    public TwoFactorEmailTokenProviderOptions()
    {
        TokenLifespan = TimeSpan.FromMinutes(3);
        TokenLength = TwoFactorEmailTokenLength.Default;
    }
}

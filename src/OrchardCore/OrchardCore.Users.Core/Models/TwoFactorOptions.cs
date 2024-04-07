using System.Collections.Generic;

namespace OrchardCore.Users.Models;

public class TwoFactorOptions
{
    /// <summary>
    /// Default token provider name used by the two-factor email provider.
    /// </summary>
    public const string TwoFactorEmailProvider = "TwoFactorEmailProvider";

    public IList<string> Providers { get; init; } = [];
}

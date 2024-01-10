using System.Collections.Generic;

namespace OrchardCore.Users.Models;

public class TwoFactorOptions
{
    public IList<string> Providers { get; } = new List<string>();
}

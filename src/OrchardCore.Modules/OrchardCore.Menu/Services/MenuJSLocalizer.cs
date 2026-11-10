using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Menu.Services;

public sealed class MenuJSLocalizer(IStringLocalizer<MenuJSLocalizer> S) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "permission-picker", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "TypeToSearch", S["Type to search"].Value },
                { "Select", S["Select"].Value },
                { "Remove", S["Remove"].Value },
                { "NoResultFound", S["No result found"].Value },
            };
        }

        return null;
    }
}

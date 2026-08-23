using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Localization.Services;

public sealed class LocalizationJSLocalizer(IStringLocalizer<LocalizationJSLocalizer> S) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "localization-culture-settings-editor", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "Culture", S["Culture"].Value },
                { "DefaultCulture", S["Default culture"].Value },
                { "SetAsDefault", S["Set as default"].Value },
                { "RemoveCulture", S["Remove culture"].Value },
                { "AddCulture", S["Add culture"].Value },
            };
        }

        return null;
    }
}

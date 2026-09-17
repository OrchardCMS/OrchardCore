using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Shortcodes.Services;

public sealed class ShortcodesJSLocalizer(IStringLocalizer<ShortcodesJSLocalizer> S) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "shortcodes-categories-editor", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "TypeToSearch", S["Type to search"].Value },
                { "Select", S["Select"].Value },
                { "Remove", S["Remove"].Value },
                { "NoCategoriesFound", S["No categories found"].Value },
                { "PressEnterToAddCategory", S["Press enter to add a category"].Value },
            };
        }

        return null;
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Taxonomies.Services;

public sealed class TaxonomiesJSLocalizer(IStringLocalizer<TaxonomiesJSLocalizer> S) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "taxonomies-tags-editor", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "TypeToSearch", S["Type to search"].Value },
                { "Select", S["Select"].Value },
                { "Remove", S["Remove"].Value },
                { "NoTagsFound", S["No tags found"].Value },
                { "PressEnterToCreateTag", S["Press enter to create a tag"].Value },
            };
        }

        return null;
    }
}

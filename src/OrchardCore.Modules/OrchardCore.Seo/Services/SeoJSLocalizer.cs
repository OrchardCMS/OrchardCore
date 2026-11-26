using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.Seo.Services;

public sealed class SeoJSLocalizer(IStringLocalizer<SeoJSLocalizer> S, IStringLocalizerFactory localizerFactory) : IJSLocalizer
{
    public IDictionary<string, string> GetLocalizations(string group)
    {
        if (string.Equals(group, "seo-meta-tags-editor", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>
            {
                { "ContentColumn", S["Content"].Value },
                { "NameColumn", localizerFactory.Localize(CommonLocalizationSources.TechnicalNames.Name).Value },
                { "PropertyColumn", S["Property"].Value },
                { "HttpEquivColumn", S["Http Equiv"].Value },
                { "CharsetColumn", S["Charset"].Value },
                { "AddACustomMetaTag", S["Add a custom meta tag"].Value },
                { "EditData", S["Edit Data"].Value },
                { "Ok", S["OK"].Value },
                { "Cancel", localizerFactory.Localize(CommonLocalizationSources.Cancel).Value },
                { "RemoveElementFromList", S["Remove element from list"].Value },
                { "CustomMetaTags", S["Custom Meta Tags"].Value },
                {
                    "CustomMetaTagsJsonHint",
                    S["A JSON representation of the allowed values, e.g. {0}", "[ { content: 'First content', name: 'name1', property: 'prop1', httpEquiv: 'http-equiv1', charset: 'charset1' }, { content: 'Second content', name: 'name2', property: 'prop2', httpEquiv: 'http-equiv2', charset: 'charset2' } ]"].Value
                },
            };
        }

        return null;
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore;

public static class CssOrchardHelperExtensions
{
    private const string PartWrapperPrefix = "content-part-wrapper";
    private const string FieldWrapperPrefix = "field-wrapper";

    public static IHtmlContent GetPartWrapperClasses(this IOrchardHelper helper, ContentTypePartDefinition partDefinition, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        var items = new List<string>()
        {
            options.WrapperClasses,
            PartWrapperPrefix,
        };

        if (partDefinition != null)
        {
            items.Add($"{PartWrapperPrefix}-{partDefinition.PartDefinition.Name.HtmlClassify()}");

            if (partDefinition.IsNamedPart())
            {
                items.Add($"{PartWrapperPrefix}-{partDefinition.Name.HtmlClassify()}");
            }
        }

        if (additionalClasses != null)
        {
            items.AddRange(additionalClasses);
        }

        return GetHtmlContentBuilder(items);
    }

    public static IHtmlContent GetFieldWrapperClasses(this IOrchardHelper helper, ContentPartFieldDefinition fieldDefinition, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        var items = new List<string>()
        {
            options.WrapperClasses,
            FieldWrapperPrefix
        };

        if (fieldDefinition != null)
        {
            items.Add($"{FieldWrapperPrefix}-{fieldDefinition.PartDefinition.Name}-{fieldDefinition.Name}".HtmlClassify());

            if (fieldDefinition.IsNamedPart())
            {
                items.Add($"{FieldWrapperPrefix}-{fieldDefinition.ContentTypePartDefinition.Name}-{fieldDefinition.Name}".HtmlClassify());
            }
        }

        if (additionalClasses != null)
        {
            items.AddRange(additionalClasses);
        }

        return GetHtmlContentBuilder(items);
    }

    private static HtmlContentBuilder GetHtmlContentBuilder(IEnumerable<string> values)
    {
        var builder = new HtmlContentBuilder();

        foreach (var value in values)
        {
            if (builder.Count == 0)
            {
                // The 'Append' method performs a string.IsNullOrEmpty() check. So, as long as the builder has no entries, we append with no spaces.
                builder.Append(value);

                continue;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            // At this point, we already know that the builder has at least one entry, so we append a single space to the class name.
            // We pass create HtmlString here to prevent the builder from preforming string.IsNullOrWhiteSpace again for performance reason.
            builder.AppendHtml(new HtmlString(' ' + value));
        }

        return builder;
    }
}

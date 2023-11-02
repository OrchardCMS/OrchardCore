using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Mvc.Utilities;
using OrchardCore.DisplayManagement.Html;
using System.Collections.Generic;

namespace OrchardCore;

public static class CssOrchardHelperExtensions
{
    private const string PartWrapperPrefix = "content-part-wrapper";
    private const string FieldWrapperPrefix = "field-wrapper";

    public static IHtmlContent GetPartWrapperClasses(this IOrchardHelper helper, ContentTypePartDefinition partDefinition, params string[] additionalClasses)
    {
        var builder = new HtmlContentBuilder();

        builder.Append(helper.GetThemeOptions().WrapperClasses);

        builder.AppendValue(PartWrapperPrefix);

        if (partDefinition?.PartDefinition != null)
        {
            builder.AppendValue($"{PartWrapperPrefix}-{partDefinition.PartDefinition.Name.HtmlClassify()}");

            if (partDefinition.IsNamedPart())
            {
                builder.AppendValue($"{PartWrapperPrefix}-{partDefinition.Name.HtmlClassify()}");
            }
        }

        return builder.AppendValues(additionalClasses);
    }

    public static IHtmlContent GetFieldWrapperClasses(this IOrchardHelper helper, ContentPartFieldDefinition fieldDefinition, params string[] additionalClasses)
    {
        var builder = new HtmlContentBuilder();

        builder.Append(helper.GetThemeOptions().WrapperClasses);

        builder.AppendValue(FieldWrapperPrefix);

        if (fieldDefinition?.PartDefinition != null)
        {
            builder.AppendValue($"{FieldWrapperPrefix}-{fieldDefinition.PartDefinition.Name}-{fieldDefinition.Name}".HtmlClassify());

            if (fieldDefinition.IsNamedPart())
            {
                builder.AppendValue($"{FieldWrapperPrefix}-{fieldDefinition.ContentTypePartDefinition.Name}-{fieldDefinition.Name}".HtmlClassify());
            }
        }

        return builder.AppendValues(additionalClasses);
    }
}

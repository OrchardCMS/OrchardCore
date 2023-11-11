using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Mvc.Utilities;
using OrchardCore.DisplayManagement.Html;

namespace OrchardCore;

public static class CssOrchardHelperExtensions
{
    private const string PartWrapperPrefix = "content-part-wrapper";
    private const string FieldWrapperPrefix = "field-wrapper";

    public static IHtmlContent GetPartWrapperClasses(this IOrchardHelper helper, ContentTypePartDefinition partDefinition, params string[] additionalClasses)
    {
        var builder = new HtmlContentBuilder();

        builder.Append(helper.GetThemeOptions().WrapperClasses);

        builder.AppendSeparatedValue(PartWrapperPrefix);

        if (partDefinition?.PartDefinition != null)
        {
            builder.AppendSeparatedValue($"{PartWrapperPrefix}-{partDefinition.PartDefinition.Name.HtmlClassify()}");

            if (partDefinition.IsNamedPart())
            {
                builder.AppendSeparatedValue($"{PartWrapperPrefix}-{partDefinition.Name.HtmlClassify()}");
            }
        }

        return builder.AppendSeparatedValues(additionalClasses);
    }

    public static IHtmlContent GetFieldWrapperClasses(this IOrchardHelper helper, ContentPartFieldDefinition fieldDefinition, params string[] additionalClasses)
    {
        var builder = new HtmlContentBuilder();

        builder.Append(helper.GetThemeOptions().WrapperClasses);

        builder.AppendSeparatedValue(FieldWrapperPrefix);

        if (fieldDefinition?.PartDefinition != null)
        {
            builder.AppendSeparatedValue($"{FieldWrapperPrefix}-{fieldDefinition.PartDefinition.Name}-{fieldDefinition.Name}".HtmlClassify());

            if (fieldDefinition.IsNamedPart())
            {
                builder.AppendSeparatedValue($"{FieldWrapperPrefix}-{fieldDefinition.ContentTypePartDefinition.Name}-{fieldDefinition.Name}".HtmlClassify());
            }
        }

        return builder.AppendSeparatedValues(additionalClasses);
    }
}

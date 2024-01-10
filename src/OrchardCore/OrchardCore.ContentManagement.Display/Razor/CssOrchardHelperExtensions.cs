using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.Mvc.Utilities;

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
            builder.AppendSeparatedValue(PartWrapperPrefix);
            builder.AppendHyphen();
            builder.Append(partDefinition.PartDefinition.Name.HtmlClassify());

            if (partDefinition.IsNamedPart())
            {
                builder.AppendSeparatedValue(PartWrapperPrefix);
                builder.AppendHyphen();
                builder.Append(partDefinition.Name.HtmlClassify());
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
            builder.AppendSeparatedValue(FieldWrapperPrefix);
            builder.AppendHyphen();
            builder.Append(fieldDefinition.PartDefinition.Name.HtmlClassify());
            builder.AppendHyphen();
            builder.Append(fieldDefinition.Name.HtmlClassify());

            if (fieldDefinition.IsNamedPart())
            {
                builder.AppendSeparatedValue(FieldWrapperPrefix);
                builder.AppendHyphen();
                builder.Append(fieldDefinition.ContentTypePartDefinition.Name.HtmlClassify());
                builder.AppendHyphen();
                builder.Append(fieldDefinition.Name.HtmlClassify());
            }
        }

        return builder.AppendSeparatedValues(additionalClasses);
    }
}

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

        CssHelpers.AppendValue(builder, PartWrapperPrefix);

        if (partDefinition != null)
        {
            CssHelpers.AppendValue(builder, $"{PartWrapperPrefix}-{partDefinition.PartDefinition.Name.HtmlClassify()}");

            if (partDefinition.IsNamedPart())
            {
                CssHelpers.AppendValue(builder, $"{PartWrapperPrefix}-{partDefinition.Name.HtmlClassify()}");
            }
        }

        if (additionalClasses != null)
        {
            foreach( var additionalClass in additionalClasses )
            {
                CssHelpers.AppendValue(builder, additionalClass);
            }
        }

        return builder;
    }

    public static IHtmlContent GetFieldWrapperClasses(this IOrchardHelper helper, ContentPartFieldDefinition fieldDefinition, params string[] additionalClasses)
    {
        var builder = new HtmlContentBuilder();

        builder.Append(helper.GetThemeOptions().WrapperClasses);

        CssHelpers.AppendValue(builder, FieldWrapperPrefix);

        if (fieldDefinition != null)
        {
            CssHelpers.AppendValue(builder, $"{FieldWrapperPrefix}-{fieldDefinition.PartDefinition.Name}-{fieldDefinition.Name}".HtmlClassify());

            if (fieldDefinition.IsNamedPart())
            {
                CssHelpers.AppendValue(builder, $"{FieldWrapperPrefix}-{fieldDefinition.ContentTypePartDefinition.Name}-{fieldDefinition.Name}".HtmlClassify());
            }
        }

        if (additionalClasses != null)
        {
            foreach (var additionalClass in additionalClasses)
            {
                CssHelpers.AppendValue(builder, additionalClass);
            }
        }

        return builder;
    }
}

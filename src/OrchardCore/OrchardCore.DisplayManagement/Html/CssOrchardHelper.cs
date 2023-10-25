using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore;

public static class CssOrchardHelper
{
    public static string GetLimitedWidthWrapperCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = GetThemeOptions(helper);

        return string.Join(' ', Combine(options.LimitedWidthWrapperClasses, additionalClasses));
    }

    public static string GetLimitedWidthCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = GetThemeOptions(helper);

        return string.Join(' ', Combine(options.LimitedWidthClasses, additionalClasses));
    }

    public static string GetLabelCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = GetThemeOptions(helper);

        return string.Join(' ', Combine(options.LabelClasses, additionalClasses));
    }

    public static string GetStartCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = GetThemeOptions(helper);

        return string.Join(' ', Combine(options.StartClasses, additionalClasses));
    }

    public static string GetEndCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = GetThemeOptions(helper);

        return string.Join(' ', Combine(options.EndClasses, additionalClasses));
    }

    public static string GetWrapperCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = GetThemeOptions(helper);

        return string.Join(' ', Combine(options.WrapperClasses, additionalClasses));
    }

    public static string GetEndCssClasses(this IOrchardHelper helper, bool withOffset, params string[] additionalClasses)
    {
        var options = GetThemeOptions(helper);

        if (withOffset && !string.IsNullOrEmpty(options.OffsetClasses))
        {
            var cssClasses = new List<string>(additionalClasses)
            {
                options.OffsetClasses
            };

            return string.Join(' ', Combine(options.EndClasses, cssClasses.ToArray()));
        }

        return string.Join(' ', Combine(options.EndClasses, additionalClasses.ToArray()));
    }

    public static string GetOffsetCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = GetThemeOptions(helper);

        return string.Join(' ', Combine(options.OffsetClasses, additionalClasses));
    }

    private static TheAdminThemeOptions GetThemeOptions(IOrchardHelper helper) => helper.HttpContext.RequestServices.GetService<IOptions<TheAdminThemeOptions>>().Value;

    private static IEnumerable<string> Combine(string optionClasses, string[] additionalClasses)
    {
        if (string.IsNullOrEmpty(optionClasses))
        {
            return additionalClasses;
        }

        return additionalClasses.Concat(new[] { optionClasses });
    }

    private static IEnumerable<string> GetPartWrapperCssClasses(ContentTypePartDefinition partDefinition)
    {
        yield return "content-part-wrapper";
        yield return $"content-part-wrapper-{partDefinition.PartDefinition.Name.HtmlClassify()}";

        if (partDefinition.IsNamedPart())
        {
            yield return $"content-part-wrapper-{partDefinition.Name.HtmlClassify()}";
        }
    }

    private static IEnumerable<string> GetFieldWrapperCssClasses(ContentPartFieldDefinition fieldDefinition)
    {
        yield return "field-wrapper";
        yield return $"field-wrapper-{fieldDefinition.GetReusableFieldWrapperClassName().HtmlClassify()}";

        if (fieldDefinition.IsNamedPart())
        {
            yield return $"field-wrapper-{fieldDefinition.GetFieldWrapperClassName().HtmlClassify()}";
        }
    }

    public static string GetPartWrapperCssClasses(this IOrchardHelper helper, ContentTypePartDefinition partDefinition, bool skipThemeWrapperClasses = false)
    {
        var partClasses = GetPartWrapperCssClasses(partDefinition).ToArray();

        return skipThemeWrapperClasses ? string.Join(' ', partClasses) : helper.GetWrapperCssClasses(partClasses);
    }

    public static string GetFieldWrapperCssClasses(this IOrchardHelper helper, ContentPartFieldDefinition fieldDefinition)
    {
        return helper.GetWrapperCssClasses(GetFieldWrapperCssClasses(fieldDefinition).ToArray());
    }
}

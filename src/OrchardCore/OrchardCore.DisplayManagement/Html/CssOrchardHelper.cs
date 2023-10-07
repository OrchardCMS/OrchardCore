using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Html;

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
}

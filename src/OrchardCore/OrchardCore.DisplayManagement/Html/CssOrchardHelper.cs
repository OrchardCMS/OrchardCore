using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Html;

namespace OrchardCore;

public static class CssOrchardHelper
{
    private static TheAdminThemeOptions _options;


    public static string GetLimitedWidthCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        _options ??= GetThemeOptions(helper);

        return String.Join(' ', Combine(_options.LimitedWidth, additionalClasses));
    }

    public static string GetLabelCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        _options ??= GetThemeOptions(helper);

        return String.Join(' ', Combine(_options.LabelClasses, additionalClasses));
    }

    public static string GetStartCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        _options ??= GetThemeOptions(helper);

        return String.Join(' ', Combine(_options.StartClasses, additionalClasses));
    }

    public static string GetEndCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        _options ??= GetThemeOptions(helper);

        return String.Join(' ', Combine(_options.EndClasses, additionalClasses));
    }

    public static string GetEndCssClasses(this IOrchardHelper helper, bool withOffset, params string[] additionalClasses)
    {
        _options ??= GetThemeOptions(helper);

        if (withOffset && !String.IsNullOrEmpty(_options.OffsetClasses))
        {
            var cssClasses = new List<string>(additionalClasses)
            {
                _options.OffsetClasses
            };

            return String.Join(' ', Combine(_options.EndClasses, cssClasses.ToArray()));
        }

        return String.Join(' ', Combine(_options.EndClasses, additionalClasses.ToArray()));
    }

    public static string GetOffsetCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        _options ??= GetThemeOptions(helper);

        return String.Join(' ', Combine(_options.OffsetClasses, additionalClasses));
    }

    private static TheAdminThemeOptions GetThemeOptions(IOrchardHelper helper) => _options ??= helper.HttpContext.RequestServices.GetService<IOptions<TheAdminThemeOptions>>().Value;

    private static IEnumerable<string> Combine(string optionClasses, string[] additionalClasses)
    {
        if (String.IsNullOrEmpty(optionClasses))
        {
            return additionalClasses;
        }

        return additionalClasses.Concat(new[] { optionClasses });
    }
}

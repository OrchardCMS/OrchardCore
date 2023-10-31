using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Html;

namespace OrchardCore;

public static class CssOrchardHelperExtensions
{
    public static IHtmlContent GetLimitedWidthWrapperClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return GetHtmlContentBuilder(options.LimitedWidthWrapperClasses, additionalClasses);
    }

    public static IHtmlContent GetLimitedWidthClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return GetHtmlContentBuilder(options.LimitedWidthClasses, additionalClasses);
    }

    public static IHtmlContent GetStartClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return GetHtmlContentBuilder(options.StartClasses, additionalClasses);
    }

    public static IHtmlContent GetEndClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return GetHtmlContentBuilder(options.EndClasses, additionalClasses);
    }

    public static IHtmlContent GetLabelClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return GetHtmlContentBuilder(options.LabelClasses, additionalClasses);
    }

    public static IHtmlContent GetWrapperClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return GetHtmlContentBuilder(options.WrapperClasses, additionalClasses);
    }

    public static IHtmlContent GetEndClasses(this IOrchardHelper helper, bool withOffset, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        if (withOffset && !string.IsNullOrEmpty(options.OffsetClasses))
        {
            var cssClasses = new List<string>(additionalClasses)
            {
                options.OffsetClasses
            };

            return GetHtmlContentBuilder(options.EndClasses, cssClasses);
        }

        return GetHtmlContentBuilder(options.EndClasses, additionalClasses);
    }

    public static IHtmlContent GetOffsetClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return GetHtmlContentBuilder(options.OffsetClasses, additionalClasses);
    }

    [Obsolete($"Please use {nameof(GetLimitedWidthWrapperClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetLimitedWidthWrapperCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return string.Join(' ', Combine(options.LimitedWidthWrapperClasses, additionalClasses));
    }

    [Obsolete($"Please use {nameof(GetLimitedWidthClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetLimitedWidthCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return string.Join(' ', Combine(options.LimitedWidthClasses, additionalClasses));
    }

    [Obsolete($"Please use {nameof(GetLabelClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetLabelCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return string.Join(' ', Combine(options.LabelClasses, additionalClasses));
    }

    [Obsolete($"Please use {nameof(GetStartClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetStartCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return string.Join(' ', Combine(options.StartClasses, additionalClasses));
    }

    [Obsolete($"Please use {nameof(GetEndClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetEndCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return string.Join(' ', Combine(options.EndClasses, additionalClasses));
    }

    [Obsolete($"Please use {nameof(GetWrapperClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetWrapperCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return string.Join(' ', Combine(options.WrapperClasses, additionalClasses));
    }

    [Obsolete($"Please use {nameof(GetEndClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetEndCssClasses(this IOrchardHelper helper, bool withOffset, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

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

    [Obsolete($"Please use {nameof(GetOffsetClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetOffsetCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        var options = helper.GetThemeOptions();

        return string.Join(' ', Combine(options.OffsetClasses, additionalClasses));
    }

    public static TheAdminThemeOptions GetThemeOptions(this IOrchardHelper helper)
        => helper.HttpContext.RequestServices.GetService<IOptions<TheAdminThemeOptions>>().Value;

    private static IEnumerable<string> Combine(string optionClasses, string[] additionalClasses)
    {
        if (string.IsNullOrEmpty(optionClasses))
        {
            return additionalClasses;
        }

        return additionalClasses.Concat(new[] { optionClasses });
    }

    private static HtmlContentBuilder GetHtmlContentBuilder(string optionClasses, IEnumerable<string> values)
    {
        var builder = new HtmlContentBuilder();

        builder.Append(optionClasses);

        if (values != null)
        {
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
        }

        return builder;
    }
}

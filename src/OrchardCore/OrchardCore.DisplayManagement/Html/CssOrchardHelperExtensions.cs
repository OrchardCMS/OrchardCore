using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Html;

namespace OrchardCore;

public static class CssOrchardHelperExtensions
{
    public static IHtmlContent GetLimitedWidthWrapperClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => GetHtmlContentBuilder(helper.GetThemeOptions().LimitedWidthWrapperClasses, additionalClasses);
    
    public static IHtmlContent GetLimitedWidthClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => GetHtmlContentBuilder(helper.GetThemeOptions().LimitedWidthClasses, additionalClasses);
    
    public static IHtmlContent GetStartClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => GetHtmlContentBuilder(helper.GetThemeOptions().StartClasses, additionalClasses);
    
    public static IHtmlContent GetEndClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => GetHtmlContentBuilder(helper.GetThemeOptions().EndClasses, additionalClasses);
    
    public static IHtmlContent GetLabelClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => GetHtmlContentBuilder(helper.GetThemeOptions().LabelClasses, additionalClasses);
    
    public static IHtmlContent GetWrapperClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => GetHtmlContentBuilder(helper.GetThemeOptions().WrapperClasses, additionalClasses);
    
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
        => GetHtmlContentBuilder(helper.GetThemeOptions().OffsetClasses, additionalClasses);
    
    [Obsolete($"Please use {nameof(GetLimitedWidthWrapperClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetLimitedWidthWrapperCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => string.Join(' ', Combine(helper.GetThemeOptions().LimitedWidthWrapperClasses, additionalClasses));
    
    [Obsolete($"Please use {nameof(GetLimitedWidthClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetLimitedWidthCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => string.Join(' ', Combine(helper.GetThemeOptions().LimitedWidthClasses, additionalClasses));
    
    [Obsolete($"Please use {nameof(GetLabelClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetLabelCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => string.Join(' ', Combine(helper.GetThemeOptions().LabelClasses, additionalClasses));
    
    [Obsolete($"Please use {nameof(GetStartClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetStartCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => string.Join(' ', Combine(helper.GetThemeOptions().StartClasses, additionalClasses));
    
    [Obsolete($"Please use {nameof(GetEndClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetEndCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => string.Join(' ', Combine(helper.GetThemeOptions().EndClasses, additionalClasses));
    
    [Obsolete($"Please use {nameof(GetWrapperClasses)} instead, as this method is deprecated and will be removed in upcoming versions.")]
    public static string GetWrapperCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
        => string.Join(' ', Combine(helper.GetThemeOptions().WrapperClasses, additionalClasses));
    

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
        => string.Join(' ', Combine(helper.GetThemeOptions().OffsetClasses, additionalClasses));
    
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

    private static HtmlContentBuilder GetHtmlContentBuilder(string optionClasses, IList<string> values)
    {
        var builder = new HtmlContentBuilder();
        
        builder.Append(optionClasses);

        builder.AppendSeparatedValues(values);

        return builder;
    }
}

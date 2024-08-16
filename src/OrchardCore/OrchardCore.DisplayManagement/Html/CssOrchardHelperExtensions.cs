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

    public static IHtmlContent GetLabelClasses(this IOrchardHelper helper, bool inputRequired = false, params string[] additionalClasses)
    {
        var themeOptions = helper.GetThemeOptions();
        var additionalClassesList = additionalClasses.ToList();

        if (inputRequired)
        {
            additionalClassesList.Add(themeOptions.LabelRequiredClasses);
        }

        return GetHtmlContentBuilder(themeOptions.LabelClasses, additionalClassesList);
    }

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

    public static TheAdminThemeOptions GetThemeOptions(this IOrchardHelper helper)
        => helper.HttpContext.RequestServices.GetService<IOptions<TheAdminThemeOptions>>().Value;

    private static HtmlContentBuilder GetHtmlContentBuilder(string optionClasses, IList<string> values)
    {
        var builder = new HtmlContentBuilder();

        builder.Append(optionClasses);

        builder.AppendSeparatedValues(values);

        return builder;
    }
}

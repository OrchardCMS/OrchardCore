using System;
using System.Collections.Generic;

namespace OrchardCore;

public static class CssOrchardHelper
{
    public static string GetLimitedWidthCssClasses(this IOrchardHelper _, params string[] additionalClasses)
    {
        return String.Join(' ', GetLimitedWidthCssClassList(additionalClasses));
    }

    public static string GetLabelCssClasses(this IOrchardHelper _, params string[] additionalClasses)
    {
        return String.Join(' ', GetLabelCssClassList(additionalClasses));
    }

    public static string GetStartCssClasses(this IOrchardHelper _, params string[] additionalClasses)
    {
        return String.Join(' ', GetStartCssClassList(additionalClasses));
    }

    public static string GetEndCssClasses(this IOrchardHelper _, params string[] additionalClasses)
    {
        return String.Join(' ', GetEndCssClassList(additionalClasses));
    }

    public static string GetEndCssClasses(this IOrchardHelper _, bool withOffset, params string[] additionalClasses)
    {
        return String.Join(' ', GetEndCssClassList(withOffset, additionalClasses));
    }

    public static string GetOffsetCssClasses(this IOrchardHelper _, params string[] additionalClasses)
    {
        return String.Join(' ', GetOffsetCssClassList(additionalClasses));
    }

    private static List<string> GetLabelCssClassList(params string[] additionalClasses)
    {
        return new List<string>(GetStartCssClassList(additionalClasses))
        {
            "col-form-label",
            "text-lg-end",
        };
    }

    private static List<string> GetLimitedWidthCssClassList(params string[] additionalClasses)
    {
        return new List<string>(additionalClasses)
        {
            "col-md-6",
            "col-lg-4",
            "col-xxl-3",
        };
    }

    private static List<string> GetStartCssClassList(params string[] additionalClasses)
    {
        return new List<string>(additionalClasses)
        {
            "col-lg-2",
            "col-xl-3"
        };
    }

    private static List<string> GetEndCssClassList(params string[] additionalClasses)
    {
        return new List<string>(additionalClasses)
        {
            "col-lg-10",
            "col-xl-9"
        };
    }

    private static List<string> GetEndCssClassList(bool withOffset, params string[] additionalClasses)
    {
        var values = GetEndCssClassList(additionalClasses);

        if (withOffset)
        {
            values.AddRange(GetOffsetCssClassList());
        }

        return values;
    }

    private static List<string> GetOffsetCssClassList(params string[] additionalClasses)
    {
        return new List<string>(additionalClasses)
        {
            "offset-lg-2",
            "offset-xl-3"
        };
    }
}

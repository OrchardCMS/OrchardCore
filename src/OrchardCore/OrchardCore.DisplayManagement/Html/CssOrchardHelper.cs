using System;
using System.Collections.Generic;

namespace OrchardCore;

public static class CssOrchardHelper
{
    public static IList<string> GetLabelCssClassList(this IOrchardHelper helper, params string[] additionalClasses)
    {
        return new List<string>(helper.GetStartCssClassList(additionalClasses))
        {
            "col-form-label",
            "text-lg-end",
        };
    }

    public static IList<string> GetControledWithCssClassList(this IOrchardHelper helper, params string[] additionalClasses)
    {
        return new List<string>(additionalClasses)
        {
            "col-md-6",
            "col-lg-4",
            "col-xxl-3",
        };
    }

    public static IList<string> GetStartCssClassList(this IOrchardHelper _, params string[] additionalClasses)
    {
        return new List<string>(additionalClasses)
        {
            "col-lg-2",
            "col-xl-3"
        };
    }

    public static IList<string> GetEndCssClassList(this IOrchardHelper _, params string[] additionalClasses)
    {
        return new List<string>(additionalClasses)
        {
            "col-lg-10",
            "col-xl-9"
        };
    }

    public static IList<string> GetEndCssClassList(this IOrchardHelper helper, bool withOffset, params string[] additionalClasses)
    {
        var values = new List<string>(helper.GetEndCssClassList(additionalClasses));

        if (withOffset)
        {
            values.AddRange(helper.GetOffsetCssClassList());
        }

        return helper.GetEndCssClassList(values.ToArray());
    }

    public static IList<string> GetOffsetCssClassList(this IOrchardHelper _, params string[] additionalClasses)
    {
        return new List<string>(additionalClasses)
        {
            "offset-lg-2",
            "offset-xl-3"
        };
    }

    public static IList<string> GetValidationErrorCssClassList(this IOrchardHelper _, params string[] additionalClasses)
    {
        return new List<string>(additionalClasses)
        {
            "has-validation-error"
        };
    }

    public static string GetControledWithCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        return String.Join(' ', helper.GetControledWithCssClassList(additionalClasses));
    }

    public static string GetLabelCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        return String.Join(' ', helper.GetLabelCssClassList(additionalClasses));
    }

    public static string GetStartCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        return String.Join(' ', helper.GetStartCssClassList(additionalClasses));
    }

    public static string GetEndCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        return String.Join(' ', helper.GetEndCssClassList(additionalClasses));
    }

    public static string GetEndCssClasses(this IOrchardHelper helper, bool withOffset, params string[] additionalClasses)
    {
        return String.Join(' ', helper.GetEndCssClassList(withOffset, additionalClasses));
    }

    public static string GetOffsetCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        return String.Join(' ', helper.GetOffsetCssClassList(additionalClasses));
    }

    public static string GetValidationErrorCssClasses(this IOrchardHelper helper, params string[] additionalClasses)
    {
        return String.Join(' ', helper.GetValidationErrorCssClassList(additionalClasses));
    }
}

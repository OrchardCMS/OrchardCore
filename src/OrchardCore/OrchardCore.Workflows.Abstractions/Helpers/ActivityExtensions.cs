using System;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Entities;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Helpers
{
    public static class ActivityExtensions
    {
        public static bool IsEvent(this IActivity activity)
        {
            return activity is IEvent;
        }

        public static LocalizedHtmlString GetTitleOrDefault(this IActivity activity, Func<LocalizedHtmlString> defaultTitle)
        {
            var title = activity.As<ActivityMetadata>().Title;
            return !string.IsNullOrEmpty(title) ? new LocalizedHtmlString(title, title) : defaultTitle();
        }
    }
}

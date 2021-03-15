using System;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Entities;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Helpers
{
    public static class ActivityExtensions
    {
        private static IHtmlLocalizer H;

        public static bool IsEvent(this IActivity activity)
        {
            return activity is IEvent;
        }

        public static LocalizedHtmlString GetTitleOrDefault(this IActivity activity, Func<LocalizedHtmlString> defaultTitle)
        {
            var title = activity.As<ActivityMetadata>().Title;
            return !string.IsNullOrEmpty(title) ? new LocalizedHtmlString(title, title) : defaultTitle();
        }

        public static LocalizedHtmlString GetLocalizedStatus(this WorkflowStatus status, IHtmlLocalizer localizer)
        {
            // Field for PoExtractor compatibility
            H = localizer;

            return status switch
            {
                WorkflowStatus.Aborted => H["Aborted"],
                WorkflowStatus.Executing => H["Executing"],
                WorkflowStatus.Faulted => H["Faulted"],
                WorkflowStatus.Finished => H["Finished"],
                WorkflowStatus.Halted => H["Halted"],
                WorkflowStatus.Idle => H["Idle"],
                WorkflowStatus.Resuming => H["Resuming"],
                WorkflowStatus.Starting => H["Starting"],
                _ => new LocalizedHtmlString(status.ToString(), status.ToString()),
            };
        }
    }
}

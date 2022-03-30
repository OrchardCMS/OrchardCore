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

            // A string used in LocalizedHtmlString won't be encoded so it needs to be pre-encoded.
            // Passing the title as an argument so it uses the HtmlEncoder when rendered
            // Another options would be to use new LocalizedHtmlString(Html.Encode(title)) but it's not available in the current context

            return !string.IsNullOrEmpty(title) ? new LocalizedHtmlString(nameof(ActivityExtensions.GetTitleOrDefault), "{0}", false, title) : defaultTitle();
        }

        public static LocalizedHtmlString GetLocalizedStatus(this IHtmlLocalizer H, WorkflowStatus status)
        {
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
                _ => throw new NotSupportedException(),
            };
        }
    }
}

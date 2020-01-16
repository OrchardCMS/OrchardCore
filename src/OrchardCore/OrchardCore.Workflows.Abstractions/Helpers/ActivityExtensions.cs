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

        public static LocalizedHtmlString GetLocalizedStatus(this WorkflowStatus status, IViewLocalizer localizer)
        {
            // A switch is used to allow string collection for translations.
            return status switch
            {
                WorkflowStatus.Aborted => localizer["Aborted"],
                WorkflowStatus.Executing => localizer["Executing"],
                WorkflowStatus.Faulted => localizer["Faulted"],
                WorkflowStatus.Finished => localizer["Finished"],
                WorkflowStatus.Halted => localizer["Halted"],
                WorkflowStatus.Idle => localizer["Idle"],
                WorkflowStatus.Resuming => localizer["Resuming"],
                WorkflowStatus.Starting => localizer["Starting"],
                _ => localizer[status.ToString()],
            };
        }
    }
}

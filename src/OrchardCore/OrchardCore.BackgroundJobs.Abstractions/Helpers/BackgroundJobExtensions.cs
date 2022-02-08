using System;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.BackgroundJobs.Models;

namespace OrchardCore.BackgroundJobs.Helpers
{
    public static class BackgroundJobExtensions
    {
        public static LocalizedHtmlString GetLocalizedStatus(this IHtmlLocalizer H, BackgroundJobStatus status)
            => status switch
            {
                BackgroundJobStatus.Scheduled => H["Scheduled"],
                BackgroundJobStatus.Queued => H["Queued"],
                BackgroundJobStatus.Executing => H["Executing"],
                BackgroundJobStatus.Executed => H["Executed"],
                BackgroundJobStatus.Retrying => H["Retrying"],
                BackgroundJobStatus.Failed => H["Failed"],
                BackgroundJobStatus.Cancelled => H["Cancelled"],
                _ => throw new NotSupportedException(),
            };
    }
}

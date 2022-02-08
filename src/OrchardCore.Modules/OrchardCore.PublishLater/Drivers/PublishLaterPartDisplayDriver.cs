using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.BackgroundJobs;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.PublishLater.Models;
using OrchardCore.PublishLater.ViewModels;

namespace OrchardCore.PublishLater.Drivers
{
    public class PublishLaterPartDisplayDriver : ContentPartDisplayDriver<PublishLaterPart>
    {
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILocalClock _localClock;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer H;

        public PublishLaterPartDisplayDriver(
            IBackgroundJobService backgroundJobService,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            ILocalClock localClock,
            INotifier notifier,
            IHtmlLocalizer<PublishLaterPartDisplayDriver> htmlLocalizer)
        {
            _backgroundJobService = backgroundJobService;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _localClock = localClock;
            _notifier = notifier;   
            H = htmlLocalizer;
        }

        public override IDisplayResult Display(PublishLaterPart part, BuildPartDisplayContext context)
        {
            return Initialize<PublishLaterPartViewModel>(
                "PublishLaterPart_SummaryAdmin",
                model => PopulateViewModel(part, model))
            .Location("SummaryAdmin", "Meta:25");
        }

        public override IDisplayResult Edit(PublishLaterPart part, BuildPartEditorContext context)
        {
            return Initialize<PublishLaterPartViewModel>(GetEditorShapeType(context),
                model => PopulateViewModel(part, model))
            .Location("Actions:10");
        }

        public override async Task<IDisplayResult> UpdateAsync(PublishLaterPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (await _authorizationService.AuthorizeAsync(httpContext?.User, CommonPermissions.PublishContent, part.ContentItem))
            {
                var viewModel = new PublishLaterPartViewModel();

                await updater.TryUpdateModelAsync(viewModel, Prefix);

                if (viewModel.ScheduledPublishLocalDateTime == null || httpContext.Request.Form["submit.Save"] == "submit.CancelPublishLater")
                {
                    part.ScheduledPublishUtc = null;
                }
                else
                {
                    part.ScheduledPublishUtc = await _localClock.ConvertToUtcAsync(viewModel.ScheduledPublishLocalDateTime.Value);
                }
                var job = await _backgroundJobService.GetByCorrelationIdAsync<PublishLaterBackgroundJob>(part.ContentItem.ContentItemId);
                if (job.BackgroundJob is null && part.ScheduledPublishUtc is not null)
                {
                    await _backgroundJobService.CreateJobAsync(
                        new PublishLaterBackgroundJob { CorrelationId = part.ContentItem.ContentItemId },
                        _backgroundJobService.Schedule.Utc(part.ScheduledPublishUtc.Value));

                }
                else if (job.BackgroundJob is not null && part.ScheduledPublishUtc is not null)
                {
                    var result = await _backgroundJobService.TryRescheduleJobAsync(job.BackgroundJob, _backgroundJobService.Schedule.Utc(part.ScheduledPublishUtc.Value));
                    if (!result.Success)
                    {
                        await _notifier.WarningAsync(H["Could not reschedule publish later"]);
                    }
                }
            }

            return Edit(part, context);
        }

        private async ValueTask PopulateViewModel(PublishLaterPart part, PublishLaterPartViewModel viewModel)
        {
            viewModel.ContentItem = part.ContentItem;
            viewModel.ScheduledPublishUtc = part.ScheduledPublishUtc;
            viewModel.ScheduledPublishLocalDateTime = part.ScheduledPublishUtc.HasValue ?
                (await _localClock.ConvertToLocalAsync(part.ScheduledPublishUtc.Value)).DateTime :
                (DateTime?)null;
        }
    }
}

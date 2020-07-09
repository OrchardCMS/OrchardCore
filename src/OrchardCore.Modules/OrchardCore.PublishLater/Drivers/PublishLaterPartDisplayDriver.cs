using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.PublishLater.Models;
using OrchardCore.PublishLater.ViewModels;

namespace OrchardCore.PublishLater.Drivers
{
    public class PublishLaterPartDisplayDriver : ContentPartDisplayDriver<PublishLaterPart>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILocalClock _localClock;

        public PublishLaterPartDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            ILocalClock localClock)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _localClock = localClock;
        }

        public override IDisplayResult Display(PublishLaterPart part, BuildPartDisplayContext context)
        {
            return Initialize<PublishLaterPartViewModel>(
                $"{nameof(PublishLaterPart)}_SummaryAdmin",
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

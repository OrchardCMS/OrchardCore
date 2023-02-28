using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ArchiveLater.Models;
using OrchardCore.ArchiveLater.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;

namespace OrchardCore.ArchiveLater.Drivers;

public class ArchiveLaterPartDisplayDriver : ContentPartDisplayDriver<ArchiveLaterPart>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILocalClock _localClock;

    public ArchiveLaterPartDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ILocalClock localClock)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _localClock = localClock;
    }

    public override IDisplayResult Display(ArchiveLaterPart part, BuildPartDisplayContext context)
        => Initialize<ArchiveLaterPartViewModel>(
            $"{nameof(ArchiveLaterPart)}_SummaryAdmin",
            model => PopulateViewModel(part, model)).Location("SummaryAdmin", "Meta:25");

    public override IDisplayResult Edit(ArchiveLaterPart part, BuildPartEditorContext context)
        => Initialize<ArchiveLaterPartViewModel>(
            GetEditorShapeType(context),
            model => PopulateViewModel(part, model)).Location("Actions:10");

    public override async Task<IDisplayResult> UpdateAsync(ArchiveLaterPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (await _authorizationService.AuthorizeAsync(httpContext?.User, CommonPermissions.PublishContent, part.ContentItem))
        {
            var viewModel = new ArchiveLaterPartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix);

            if (viewModel.ScheduledArchiveLocalDateTime == null || httpContext.Request.Form["submit.Publish"] == "submit.CancelArchiveLater")
            {
                part.ScheduledArchiveUtc = null;
            }
            else
            {
                part.ScheduledArchiveUtc = await _localClock.ConvertToUtcAsync(viewModel.ScheduledArchiveLocalDateTime.Value);
            }
        }

        return Edit(part, context);
    }

    private async ValueTask PopulateViewModel(ArchiveLaterPart part, ArchiveLaterPartViewModel viewModel)
    {
        viewModel.ContentItem = part.ContentItem;
        viewModel.ScheduledArchiveUtc = part.ScheduledArchiveUtc;
        viewModel.ScheduledArchiveLocalDateTime = part.ScheduledArchiveUtc.HasValue
            ? (await _localClock.ConvertToLocalAsync(part.ScheduledArchiveUtc.Value)).DateTime
            : (DateTime?)null;
    }
}

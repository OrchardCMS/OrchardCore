using Microsoft.AspNetCore.Authorization;
using OrchardCore.AuditTrail;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.AuditTrail.Drivers;

public sealed class AuditTrailContentsDriver : ContentDisplayDriver
{
    private readonly IAuthorizationService _authorizationService;

    public AuditTrailContentsDriver(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public override Task<IDisplayResult> DisplayAsync(ContentItem contentItem, BuildDisplayContext context)
    {
        return Task.FromResult<IDisplayResult>(
            Initialize<ContentItemViewModel>("AuditTrailContentsAction_SummaryAdmin", m => m.ContentItem = contentItem)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:10")
            .RenderWhen(() => _authorizationService.AuthorizeAsync(context.HttpContext?.User, AuditTrailPermissions.ViewAuditTrail))
        );
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.AuditTrail.Drivers;
using OrchardCore.AuditTrail.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.ViewModels;

namespace OrchardCore.Users.AuditTrail.Drivers;

public sealed class AuditTrailUserEventDisplayDriver : AuditTrailEventSectionDisplayDriver<AuditTrailUserEvent>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditTrailUserEventDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override IDisplayResult Display(AuditTrailEvent auditTrailEvent, AuditTrailUserEvent userEvent, BuildDisplayContext context) =>
        Initialize<AuditTrailUserEventViewModel>("AuditTrailUserEventDetail_DetailAdmin", model =>
            {
                model.AuditTrailEvent = auditTrailEvent;
                model.UserEvent = userEvent;
            })
            .RenderWhen(async () =>
                _httpContextAccessor.HttpContext?.User is { Identity.IsAuthenticated: true } claimsPrincipal &&
                await _authorizationService.AuthorizeAsync(claimsPrincipal, Permissions.ViewUserAuditTrailEvents, userEvent))
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:10");
}

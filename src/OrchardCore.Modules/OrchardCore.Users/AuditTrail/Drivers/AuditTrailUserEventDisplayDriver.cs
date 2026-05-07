using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OrchardCore.AuditTrail.Drivers;
using OrchardCore.AuditTrail.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Users.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.ViewModels;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.AuditTrail.Drivers;

public sealed class AuditTrailUserEventDisplayDriver : AuditTrailEventSectionDisplayDriver<AuditTrailUserEvent>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IUser> _userManager;

    public AuditTrailUserEventDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        UserManager<IUser> userManager)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
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

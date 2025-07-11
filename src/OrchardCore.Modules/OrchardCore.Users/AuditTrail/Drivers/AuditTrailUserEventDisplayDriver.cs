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
    readonly IAuthorizationService _authorizationService;
    readonly IHttpContextAccessor _httpContextAccessor;
    readonly UserManager<IUser> _userManager;

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
            {
                if (_httpContextAccessor.HttpContext?.User is not { Identity.IsAuthenticated: true } claimsPrincipal)
                {
                    return false;
                }

                var permission = await _userManager.GetUserAsync(claimsPrincipal) is User { UserId: { } userId } &&
                                 userId.EqualsOrdinalIgnoreCase(userEvent?.UserId)
                    ? Permissions.ViewOwnUserAuditTrailEvents
                    : Permissions.ViewUserAuditTrailEvents; 
                    
                return await _authorizationService.AuthorizeAsync(claimsPrincipal, permission);
            })
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:10");
}

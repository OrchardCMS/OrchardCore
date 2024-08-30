using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Notifications.Drivers;

public sealed class NotificationNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NotificationNavbarDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return Dynamic("NotificationNavbarWrapper")
            .Location("Detail", "Content:9")
            .Location("DetailAdmin", "Content:9")
            .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, NotificationPermissions.ManageNotifications));
    }
}

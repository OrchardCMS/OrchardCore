using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.CompiledQueries;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public sealed class NotificationNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly NotificationOptions _notificationOptions;
    private readonly YesSql.ISession _session;

    public NotificationNavbarDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IOptions<NotificationOptions> notificationOptions,
        YesSql.ISession session)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _notificationOptions = notificationOptions.Value;
        _session = session;
    }

    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return Initialize<UserNotificationNavbarViewModel>("UserNotificationNavbar", async model =>
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = (await _session.ExecuteQuery(new QueryTopUnreadNotificationsByUserId(userId, _notificationOptions.TotalUnreadNotifications + 1), collection: NotificationConstants.NotificationCollection).ListAsync()).ToList();

            model.Notifications = notifications;
            model.MaxVisibleNotifications = _notificationOptions.TotalUnreadNotifications;
            model.TotalUnread = notifications.Count;

        }).Location("Detail", "Content:9")
        .Location("DetailAdmin", "Content:9")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, NotificationPermissions.ManageNotifications));
    }
}

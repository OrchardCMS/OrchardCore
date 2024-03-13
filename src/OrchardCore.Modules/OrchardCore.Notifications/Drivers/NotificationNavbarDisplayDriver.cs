using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.ViewModels;
using YesSql;

namespace OrchardCore.Notifications.Drivers;

public class NotificationNavbarDisplayDriver : DisplayDriver<Navbar>
{
    // TODO, make this part of a configurable of NotificationOptions
    private const int MaxVisibleNotifications = 10;

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly YesSql.ISession _session;

    public NotificationNavbarDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        YesSql.ISession session)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _session = session;
    }

    public override IDisplayResult Display(Navbar model)
    {
        return Initialize<UserNotificationNavbarViewModel>("UserNotificationNavbar", async model =>
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = (await _session.Query<Notification, NotificationIndex>(x => x.UserId == userId && !x.IsRead, collection: NotificationConstants.NotificationCollection)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(MaxVisibleNotifications + 1)
                .ListAsync()).ToList();

            model.Notifications = notifications;
            model.MaxVisibleNotifications = MaxVisibleNotifications;
            model.TotalUnread = notifications.Count;

        }).Location("Detail", "Content:9")
        .Location("DetailAdmin", "Content:9")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, NotificationPermissions.ManageNotifications));
    }
}

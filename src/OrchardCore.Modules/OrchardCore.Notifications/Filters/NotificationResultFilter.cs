using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.ViewModels;
using YesSql;

namespace OrchardCore.Notifications.Filters;

public class NotificationResultFilter : IAsyncResultFilter
{
    // TODO, make this part of a configurable of NotificationOptions
    private const int MaxVisibleNotifications = 10;

    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDisplayManager<Notification> _notificationDisplayDriver;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly ISession _session;

    public NotificationResultFilter(ILayoutAccessor layoutAccessor,
        IAuthorizationService authorizationService,
        IDisplayManager<Notification> notificationDisplayDriver,
        IUpdateModelAccessor updateModelAccessor,
        ISession session)
    {
        _layoutAccessor = layoutAccessor;
        _authorizationService = authorizationService;
        _notificationDisplayDriver = notificationDisplayDriver;
        _updateModelAccessor = updateModelAccessor;
        _session = session;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is not (ViewResult or PageResult)
            || !await _authorizationService.AuthorizeAsync(context.HttpContext.User, NotificationPermissions.ManageNotifications))
        {
            await next();

            return;
        }

        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notifications = await _session.Query<Notification, NotificationIndex>(x => x.UserId == userId && !x.IsRead, collection: NotificationConstants.NotificationCollection)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(MaxVisibleNotifications + 1)
            .ListAsync();

        var shapes = new List<dynamic>();

        foreach (var notification in notifications)
        {
            dynamic shape = await _notificationDisplayDriver.BuildDisplayAsync(notification, _updateModelAccessor.ModelUpdater, "Header");
            shape.Notification = notification;

            shapes.Add(shape);
        }

        var viewModel = new UserNotificationCollectionViewModel()
        {
            TotalUnread = notifications.Count(),
            MaxVisibleNotifications = MaxVisibleNotifications,
            Notifications = shapes,
        };

        var layout = await _layoutAccessor.GetLayoutAsync();
        var contentZone = layout.Zones["NavbarTop"];

        await contentZone.AddAsync(new ShapeViewModel<UserNotificationCollectionViewModel>("UserNotificationCollection", viewModel));

        await next();
    }
}

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.ViewModels;
using YesSql;

namespace OrchardCore.Notifications.Filters;

public class WebNotificationResultFilter : IAsyncResultFilter
{
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly YesSql.ISession _session;

    public WebNotificationResultFilter(ILayoutAccessor layoutAccessor,
        IAuthorizationService authorizationService,
        YesSql.ISession session)
    {
        _layoutAccessor = layoutAccessor;
        _authorizationService = authorizationService;
        _session = session;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is not (ViewResult or PageResult))
        {
            await next();

            return;
        }

        if (!await _authorizationService.AuthorizeAsync(context.HttpContext.User, WebNotificationPermission.ManageOwnNotifications))
        {
            await next();

            return;
        }
        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notifications = await _session.QueryIndex<WebNotificationMessageIndex>(x => x.UserId == userId && !x.IsRead)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(11)
            .ListAsync();

        var viewModel = new UserWebNotificationViewModel()
        {
            TotalUnread = notifications.Count(),
            Notifications = notifications.Select(x => new UserWebNotificationMessageViewModel()
            {
                MessageId = x.ContentItemId,
                IsRead = x.IsRead,
                Subject = x.Subject,
                Body = x.Body,
            }),
        };

        var layout = await _layoutAccessor.GetLayoutAsync();
        var contentZone = layout.Zones["NavbarTop"];

        await contentZone.AddAsync(new ShapeViewModel<UserWebNotificationViewModel>("UserWebNotification", viewModel));

        await next();
    }

}

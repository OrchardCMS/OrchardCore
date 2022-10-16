using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.ViewModels;
using YesSql;

namespace OrchardCore.Notifications.Controllers;

public class AdminController : Controller, IUpdateModel
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISession _session;
    private readonly dynamic New;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly PagerOptions _pagerOptions;
    private readonly IClock _clock;

    public AdminController(
        IAuthorizationService authorizationService,
        ISession session,
        IShapeFactory shapeFactory,
        IOptions<PagerOptions> pagerOptions,
        IContentItemDisplayManager contentItemDisplayManager,
        IClock clock)
    {
        _authorizationService = authorizationService;
        _session = session;
        New = shapeFactory;
        _contentItemDisplayManager = contentItemDisplayManager;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;
        _clock = clock;
    }

    public async Task<IActionResult> List(PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, WebNotificationPermissions.ManageWebNotifications))
        {
            return Forbid();
        }

        var query = _session.Query<ContentItem, WebNotificationIndex>(x => x.UserId == CurrentUserId());

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());
        var pagerShape = (await New.Pager(pager)).TotalItemCount(_pagerOptions.MaxPagedCount > 0 ? _pagerOptions.MaxPagedCount : await query.CountAsync());

        var notifications = await query.OrderByDescending(x => x.CreatedAtUtc).Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();

        var contentItemSummaries = new List<dynamic>();
        foreach (var notificaiton in notifications)
        {
            contentItemSummaries.Add(await _contentItemDisplayManager.BuildDisplayAsync(notificaiton, this, "SummaryAdmin"));
        }

        var shapeViewModel = await _shapeFactory.CreateAsync<ListNotificationsViewModel>("NotificationsAdminList", viewModel =>
        {
            viewModel.Notifications = contentItemSummaries;
            viewModel.Pager = pagerShape;
        });

        return View(shapeViewModel);
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}

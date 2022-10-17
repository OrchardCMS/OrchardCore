using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Navigation.Core;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;
using YesSql;
using YesSql.Filters.Query;

namespace OrchardCore.Notifications.Controllers;

[Feature("OrchardCore.Notifications.Web")]
public class AdminController : Controller, IUpdateModel
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISession _session;
    private readonly dynamic New;
    private readonly IDisplayManager<WebNotification> _webNoticiationDisplayManager;
    private readonly INotificationsAdminListQueryService _notificationsAdminListQueryService;
    private readonly IDisplayManager<ListNotificationOptions> _notificationOptionsDisplayManager;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer H;
    private readonly IShapeFactory _shapeFactory;
    private readonly PagerOptions _pagerOptions;
    private readonly IClock _clock;

    public AdminController(
        IAuthorizationService authorizationService,
        ISession session,
        IShapeFactory shapeFactory,
        IOptions<PagerOptions> pagerOptions,
        IDisplayManager<WebNotification> webNoticiationDisplayManager,
        INotificationsAdminListQueryService notificationsAdminListQueryService,
        IDisplayManager<ListNotificationOptions> notificationOptionsDisplayManager,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IClock clock)
    {
        _authorizationService = authorizationService;
        _session = session;
        New = shapeFactory;
        _webNoticiationDisplayManager = webNoticiationDisplayManager;
        _notificationsAdminListQueryService = notificationsAdminListQueryService;
        _notificationOptionsDisplayManager = notificationOptionsDisplayManager;
        _notifier = notifier;
        H = htmlLocalizer;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;
        _clock = clock;
    }

    public async Task<IActionResult> List([ModelBinder(BinderType = typeof(WebNotificationFilterEngineModelBinder), Name = "q")] QueryFilterResult<WebNotification> queryFilterResult,
        PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, WebNotificationPermissions.ManageWebNotifications))
        {
            return Forbid();
        }

        var options = new ListNotificationOptions
        {
            FilterResult = queryFilterResult
        };

        options.FilterResult.MapTo(options);

        // Populate route values to maintain previous route data when generating page links.
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        var routeData = new RouteData(options.RouteValues);

        var query = await _notificationsAdminListQueryService.QueryAsync(options, this);

        // The search text is provided back to the UI.
        options.SearchText = options.FilterResult.ToString();
        options.OriginalSearchText = options.SearchText;

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());
        var pagerShape = (await New.Pager(pager)).TotalItemCount(_pagerOptions.MaxPagedCount > 0 ? _pagerOptions.MaxPagedCount : await query.CountAsync()).RouteData(routeData);

        var notifications = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();

        var contentItemSummaries = new List<dynamic>();
        foreach (var notificaiton in notifications)
        {
            dynamic shape = await _webNoticiationDisplayManager.BuildDisplayAsync(notificaiton, this, "SummaryAdmin");
            shape.WebNotification = notificaiton;

            contentItemSummaries.Add(shape);
        }

        var startIndex = (pagerShape.Page - 1) * pagerShape.PageSize + 1;
        options.StartIndex = startIndex;
        options.EndIndex = startIndex + contentItemSummaries.Count - 1;
        options.ContentItemsCount = contentItemSummaries.Count;
        options.TotalItemCount = pagerShape.TotalItemCount;

        var header = await _notificationOptionsDisplayManager.BuildEditorAsync(options, this, false);

        var shapeViewModel = await _shapeFactory.CreateAsync<ListNotificationsViewModel>("NotificationsAdminList", viewModel =>
        {
            viewModel.Header = header;
            viewModel.Notifications = contentItemSummaries;
            viewModel.Pager = pagerShape;
        });

        return View(shapeViewModel);
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<IActionResult> ReadAll(string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, WebNotificationPermissions.ManageWebNotifications))
        {
            return Forbid();
        }

        var records = await _session.Query<WebNotification, WebNotificationIndex>(x => x.UserId == CurrentUserId() && !x.IsRead, collection: WebNotification.Collection).ListAsync();
        var utcNow = _clock.UtcNow;
        foreach (var record in records)
        {
            record.IsRead = true;
            record.ReadAtUtc = utcNow;

            _session.Save(record, collection: WebNotification.Collection);
        }

        return Url.IsLocalUrl(returnUrl) ? (IActionResult)this.LocalRedirect(returnUrl, true) : RedirectToAction(nameof(List));
    }
}

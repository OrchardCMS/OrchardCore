using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Navigation.Core;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.Services;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Routing;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.Notifications.Controllers;

public sealed class AdminController : Controller, IUpdateModel
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISession _session;
    private readonly ITagCache _tagCache;
    private readonly IDisplayManager<Notification> _notificationDisplayManager;
    private readonly INotificationsAdminListQueryService _notificationsAdminListQueryService;
    private readonly IDisplayManager<ListNotificationOptions> _notificationOptionsDisplayManager;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;
    private readonly PagerOptions _pagerOptions;
    private readonly IClock _clock;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IAuthorizationService authorizationService,
        ISession session,
        ITagCache tagCache,
        IOptions<PagerOptions> pagerOptions,
        IDisplayManager<Notification> notificationDisplayManager,
        INotificationsAdminListQueryService notificationsAdminListQueryService,
        IDisplayManager<ListNotificationOptions> notificationOptionsDisplayManager,
        INotifier notifier,
        IClock clock,
        IShapeFactory shapeFactory,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _session = session;
        _tagCache = tagCache;
        _notificationDisplayManager = notificationDisplayManager;
        _notificationsAdminListQueryService = notificationsAdminListQueryService;
        _notificationOptionsDisplayManager = notificationOptionsDisplayManager;
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;
        _clock = clock;

        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("notifications", "ListNotifications")]
    public async Task<IActionResult> List(
        [ModelBinder(BinderType = typeof(NotificationFilterEngineModelBinder), Name = "q")] QueryFilterResult<Notification> queryFilterResult,
        PagerParameters pagerParameters,
        ListNotificationOptions options)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, NotificationPermissions.ManageNotifications))
        {
            return Forbid();
        }

        options.FilterResult = queryFilterResult;

        // The search text is provided back to the UI.
        options.SearchText = options.FilterResult.ToString();
        options.OriginalSearchText = options.SearchText;

        // Populate route values to maintain previous route data when generating page links.
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        options.Statuses =
        [
            new(S["Read"], nameof(NotificationStatus.Read)),
            new(S["Unread"], nameof(NotificationStatus.Unread)),
        ];
        options.Sorts =
        [
            new(S["Recently created"], nameof(NotificationOrder.Latest)),
            new(S["Previously created"], nameof(NotificationOrder.Oldest)),
        ];
        options.BulkActions =
        [
            new(S["Mark as read"], nameof(NotificationBulkAction.Read)),
            new(S["Mark as unread"], nameof(NotificationBulkAction.Unread)),
            new(S["Remove"], nameof(NotificationBulkAction.Remove)),
        ];

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

        var queryResult = await _notificationsAdminListQueryService.QueryAsync(pager.Page, pager.PageSize, options, this);

        var pagerShape = await _shapeFactory.PagerAsync(pager, queryResult.TotalCount, options.RouteValues);

        var notificationShapes = new List<IShape>();

        foreach (var notification in queryResult.Notifications)
        {
            var shape = await _notificationDisplayManager.BuildDisplayAsync(notification, this, "SummaryAdmin");
            shape.Properties[nameof(Notification)] = notification;

            notificationShapes.Add(shape);
        }

        var startIndex = (pager.Page - 1) * pager.PageSize + 1;
        options.StartIndex = startIndex;
        options.EndIndex = startIndex + notificationShapes.Count - 1;
        options.NotificationsCount = notificationShapes.Count;
        options.TotalItemCount = queryResult.TotalCount;

        var header = await _notificationOptionsDisplayManager.BuildEditorAsync(options, this, false, string.Empty, string.Empty);

        var shapeViewModel = await _shapeFactory.CreateAsync<ListNotificationsViewModel>("NotificationsAdminList", viewModel =>
        {
            viewModel.Options = options;
            viewModel.Header = header;
            viewModel.Notifications = notificationShapes;
            viewModel.Pager = pagerShape;
        });

        return View(shapeViewModel);
    }

    [HttpPost]
    [ActionName(nameof(List))]
    [FormValueRequired("submit.Filter")]
    public async Task<ActionResult> ListFilterPOST(ListNotificationOptions options)
    {
        // When the user has typed something into the search input, no further evaluation of the form post is required.
        if (!string.Equals(options.SearchText, options.OriginalSearchText, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(List), new RouteValueDictionary { { "q", options.SearchText } });
        }

        // Evaluate the values provided in the form post and map them to the filter result and route values.
        await _notificationOptionsDisplayManager.UpdateEditorAsync(options, this, false, string.Empty, string.Empty);

        // The route value must always be added after the editors have updated the models.
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        return RedirectToAction(nameof(List), options.RouteValues);
    }

    [HttpPost]
    [ActionName(nameof(List))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> ListPOST(ListNotificationOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, NotificationPermissions.ManageNotifications))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var notifications = await _session.Query<Notification, NotificationIndex>(x => x.UserId == CurrentUserId() && x.NotificationId.IsIn(itemIds), collection: NotificationConstants.NotificationCollection).ListAsync();
            var utcNow = _clock.UtcNow;
            var counter = 0;

            switch (options.BulkAction)
            {
                case NotificationBulkAction.Unread:
                    foreach (var notification in notifications)
                    {
                        var readPart = notification.As<NotificationReadInfo>();
                        if (readPart.IsRead)
                        {
                            readPart.IsRead = false;
                            readPart.ReadAtUtc = null;

                            notification.Put(readPart);

                            await _session.SaveAsync(notification, collection: NotificationConstants.NotificationCollection);
                            counter++;
                        }
                    }
                    if (counter > 0)
                    {
                        await _tagCache.RemoveTagAsync(NotificationsHelper.GetUnreadUserNotificationTagKey(User.Identity.Name));
                        await _notifier.SuccessAsync(H["{0} {1} unread successfully.", counter, H.Plural(counter, "notification", "notifications")]);
                    }
                    break;
                case NotificationBulkAction.Read:
                    foreach (var notification in notifications)
                    {
                        var readPart = notification.As<NotificationReadInfo>();

                        if (!readPart.IsRead)
                        {
                            readPart.IsRead = true;
                            readPart.ReadAtUtc = null;

                            notification.Put(readPart);

                            await _session.SaveAsync(notification, collection: NotificationConstants.NotificationCollection);
                            counter++;
                        }
                    }
                    if (counter > 0)
                    {
                        await _tagCache.RemoveTagAsync(NotificationsHelper.GetUnreadUserNotificationTagKey(User.Identity.Name));
                        await _notifier.SuccessAsync(H["{0} {1} read successfully.", counter, H.Plural(counter, "notification", "notifications")]);
                    }
                    break;
                case NotificationBulkAction.Remove:
                    foreach (var notification in notifications)
                    {
                        _session.Delete(notification, collection: NotificationConstants.NotificationCollection);
                        counter++;
                    }
                    if (counter > 0)
                    {
                        await _tagCache.RemoveTagAsync(NotificationsHelper.GetUnreadUserNotificationTagKey(User.Identity.Name));
                        await _notifier.SuccessAsync(H["{0} {1} removed successfully.", counter, H.Plural(counter, "notification", "notifications")]);
                    }
                    break;
                default:
                    break;
            }
        }

        return RedirectToAction(nameof(List));
    }

    public async Task<IActionResult> ReadAll(string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, NotificationPermissions.ManageNotifications))
        {
            return Forbid();
        }

        var notifications = await _session.Query<Notification, NotificationIndex>(x => x.UserId == CurrentUserId() && !x.IsRead, collection: NotificationConstants.NotificationCollection).ListAsync();

        var utcNow = _clock.UtcNow;
        var counter = 0;
        foreach (var notification in notifications)
        {
            var readPart = notification.As<NotificationReadInfo>();

            readPart.IsRead = true;
            readPart.ReadAtUtc = utcNow;

            notification.Put(readPart);
            await _session.SaveAsync(notification, collection: NotificationConstants.NotificationCollection);
            counter++;
        }

        if (counter > 0)
        {
            await _tagCache.RemoveTagAsync(NotificationsHelper.GetUnreadUserNotificationTagKey(User.Identity.Name));
            await _notifier.SuccessAsync(H["{0} {1} read successfully.", counter, H.Plural(counter, "notification", "notifications")]);
        }

        return RedirectTo(returnUrl);
    }

    public async Task<IActionResult> Toggle(string notificationId, bool markAsRead, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, NotificationPermissions.ManageNotifications))
        {
            return Forbid();
        }

        if (!string.IsNullOrWhiteSpace(notificationId))
        {
            var notification = await _session.Query<Notification, NotificationIndex>(x => x.UserId == CurrentUserId() && x.NotificationId == notificationId && x.IsRead != markAsRead, collection: NotificationConstants.NotificationCollection).FirstOrDefaultAsync();

            if (notification != null)
            {
                var readPart = notification.As<NotificationReadInfo>();

                if (markAsRead)
                {
                    readPart.IsRead = true;
                    readPart.ReadAtUtc = _clock.UtcNow;
                }
                else
                {
                    readPart.IsRead = false;
                    readPart.ReadAtUtc = null;
                }

                notification.Put(readPart);

                await _session.SaveAsync(notification, collection: NotificationConstants.NotificationCollection);
                await _tagCache.RemoveTagAsync(NotificationsHelper.GetUnreadUserNotificationTagKey(User.Identity.Name));
            }
        }

        return RedirectTo(returnUrl);
    }

    public async Task<IActionResult> Delete(string notificationId, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(HttpContext.User, NotificationPermissions.ManageNotifications))
        {
            return Forbid();
        }

        if (!string.IsNullOrWhiteSpace(notificationId))
        {
            var notification = await _session.Query<Notification, NotificationIndex>(x => x.UserId == CurrentUserId() && x.NotificationId == notificationId, collection: NotificationConstants.NotificationCollection).FirstOrDefaultAsync();

            if (notification != null)
            {
                _session.Delete(notification, collection: NotificationConstants.NotificationCollection);
                await _tagCache.RemoveTagAsync(NotificationsHelper.GetUnreadUserNotificationTagKey(User.Identity.Name));
            }
        }

        return RedirectTo(returnUrl);
    }

    private IActionResult RedirectTo(string returnUrl)
        => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
        ? (IActionResult)this.LocalRedirect(returnUrl, true)
        : RedirectToAction(nameof(List));

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier);
}

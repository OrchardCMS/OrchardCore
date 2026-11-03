using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;
using YesSql;

namespace OrchardCore.Notifications.Drivers;

public sealed class NotificationNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly INotificationVisibilityEvaluator _visibilityEvaluator;
    private readonly NotificationOptions _notificationOptions;
    private readonly YesSql.ISession _session;

    public NotificationNavbarDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        INotificationVisibilityEvaluator visibilityEvaluator,
        IOptions<NotificationOptions> notificationOptions,
        YesSql.ISession session)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _visibilityEvaluator = visibilityEvaluator;
        _notificationOptions = notificationOptions.Value;
        _session = session;
    }

    public override async Task<IDisplayResult> DisplayAsync(Navbar model, BuildDisplayContext context)
    {
        var user = _httpContextAccessor.HttpContext.User;

        if (!await _authorizationService.AuthorizeAsync(user, NotificationPermissions.ManageNotifications))
        {
            return null;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = Initialize<UserNotificationNavbarViewModel>("UserNotificationNavbar")
            .Processing<UserNotificationNavbarViewModel>(async model =>
            {
                model.MaxVisibleNotifications = _notificationOptions.TotalUnreadNotifications;

                if (_visibilityEvaluator.HasHandlers)
                {
                    await PopulateVisibleAsync(model, userId, user);

                    return;
                }

                var notifications = (await _session.Query<Notification, NotificationIndex>(x => x.UserId == userId && !x.IsRead, collection: NotificationConstants.NotificationCollection)
                    .OrderByDescending(x => x.CreatedAtUtc)
                    .Take(_notificationOptions.TotalUnreadNotifications)
                    .ListAsync()).ToList();

                model.Notifications = notifications;
                model.TotalUnread = notifications.Count < _notificationOptions.TotalUnreadNotifications
                ? notifications.Count
                : await _session.QueryIndex<NotificationIndex>(x => x.UserId == userId && !x.IsRead, collection: NotificationConstants.NotificationCollection).CountAsync();

            }).Location(OrchardCoreConstants.DisplayType.Detail, "Content:9")
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:9");

        if (_notificationOptions.AbsoluteCacheExpirationSeconds > 0 || _notificationOptions.SlidingCacheExpirationSeconds > 0)
        {
            result
                .Cache(NotificationConstants.TopUnreadUserNotificationCacheTag, context =>
                {
                    context.AddContext("user")
                        // Allow another feature to clear all notification cache entries if necessary.
                        .AddTag(NotificationConstants.TopUnreadUserNotificationCacheTag)
                        .AddTag(NotificationsHelper.GetUnreadUserNotificationTagKey(user.Identity.Name))
                        .AddTag(NotificationsHelper.GetUnreadUserNotificationTagKeyByUserId(userId));

                    if (_notificationOptions.AbsoluteCacheExpirationSeconds > 0)
                    {
                        context.WithExpiryAfter(TimeSpan.FromSeconds(_notificationOptions.AbsoluteCacheExpirationSeconds));
                    }

                    if (_notificationOptions.SlidingCacheExpirationSeconds > 0)
                    {
                        context.WithExpirySliding(TimeSpan.FromSeconds(_notificationOptions.SlidingCacheExpirationSeconds));
                    }
                });
        }

        // A cached shape doesn't register its resources when its content comes from the cache, so they have their own shape.
        return Combine(
            result,
            Dynamic("UserNotificationNavbarResources")
                .Location(OrchardCoreConstants.DisplayType.Detail, "Content:9")
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:9"));
    }

    // The hidden notifications are neither shown nor counted, so the unread notifications are evaluated
    // before they are counted, up to the configured maximum.
    private async Task PopulateVisibleAsync(UserNotificationNavbarViewModel model, string userId, ClaimsPrincipal user)
    {
        var maxEvaluated = Math.Max(_notificationOptions.MaxEvaluatedUnreadNotifications, _notificationOptions.TotalUnreadNotifications);

        // One more is loaded to know whether the user has more unread notifications than the evaluated ones.
        var unread = (await _session.Query<Notification, NotificationIndex>(x => x.UserId == userId && !x.IsRead, collection: NotificationConstants.NotificationCollection)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(maxEvaluated + 1)
            .ListAsync()).ToList();

        var visible = await _visibilityEvaluator.FilterAsync(unread.Take(maxEvaluated), user);

        model.Notifications = visible.Take(_notificationOptions.TotalUnreadNotifications).ToList();
        model.TotalUnread = visible.Count;
        model.IsTotalUnreadCapped = unread.Count > maxEvaluated;
    }
}

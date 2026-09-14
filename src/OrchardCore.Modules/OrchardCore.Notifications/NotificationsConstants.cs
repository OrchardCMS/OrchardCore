using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Notifications;

/// <summary>
/// The name of the breadcrumb rendered by the notifications screen.
/// </summary>
public static class NotificationsConstants
{
    /// <summary>
    /// The breadcrumb of the notifications screen.
    /// </summary>
    public const string List = "Notifications";
}

/// <summary>
/// Describes the breadcrumb trail of the notifications screen.
/// </summary>
public sealed class NotificationsBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public NotificationsBreadcrumbProvider(IStringLocalizer<NotificationsBreadcrumbProvider> stringLocalizer)
        : base(NotificationsConstants.List)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        builder.Add(S["Notification Center"], item => item.Id("Notifications"));

        return ValueTask.CompletedTask;
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Utilities;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public sealed class NotificationDisplayDriver : DisplayDriver<Notification>
{
    private readonly INotificationVisibilityEvaluator _visibilityEvaluator;
    private readonly INotificationManager _notificationManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly NotificationTypeOptions _notificationTypeOptions;

    public NotificationDisplayDriver(
        INotificationVisibilityEvaluator visibilityEvaluator,
        INotificationManager notificationManager,
        IHttpContextAccessor httpContextAccessor,
        IOptions<NotificationTypeOptions> notificationTypeOptions)
    {
        _visibilityEvaluator = visibilityEvaluator;
        _notificationManager = notificationManager;
        _httpContextAccessor = httpContextAccessor;
        _notificationTypeOptions = notificationTypeOptions.Value;
    }

    public override async Task<IDisplayResult> DisplayAsync(Notification notification, BuildDisplayContext context)
    {
        var isRead = _notificationManager.IsRead(notification);
        var visibility = await _visibilityEvaluator.EvaluateAsync(notification, _httpContextAccessor.HttpContext?.User);

        if (context.Shape is Shape shape)
        {
            shape.Properties[nameof(Notification)] = notification;
            shape.Properties[nameof(NotificationVisibility)] = visibility;

            if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid)
            {
                shape.Classes.Add(isRead ? NotificationsAdminList.ReadCssClass : NotificationsAdminList.UnreadCssClass);
            }

            // Lets a module render its own notifications with a template, for example 'Notification-Mention.Header.cshtml'.
            if (!string.IsNullOrEmpty(notification.Type))
            {
                var encodedType = notification.Type.EncodeAlternateElement();

                shape.Metadata.Alternates.Add($"Notification__{encodedType}");
                shape.Metadata.Alternates.Add($"{shape.Metadata.Type}__{encodedType}");
            }
        }

        _notificationTypeOptions.Types.TryGetValue(notification.Type ?? string.Empty, out var typeDescriptor);

        var model = new NotificationViewModel(notification)
        {
            IsRead = isRead,
            Visibility = visibility,
            TypeDescriptor = typeDescriptor,
        };

        var results = new List<IDisplayResult>
        {
            CreateShape("NotificationsCheckbox_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"),
            CreateShape("NotificationsMeta_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:20")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Received:20")
                .Location(NotificationConstants.DisplayTypes.Header, "Meta:20")
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Meta:20"),
            CreateShape("NotificationsActions_DetailAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Actions:5"),
            CreateShape("NotificationsActions_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
            CreateShape("NotificationsButtonActions_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:10"),
        };

        if (visibility == NotificationVisibility.Restricted)
        {
            results.Add(CreateShape("NotificationsRestricted_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Subject:1")
                .Location(NotificationConstants.DisplayTypes.Header, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1"));
        }
        else
        {
            results.Add(CreateShape("NotificationsSubject_SummaryAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Subject:1"));
            results.Add(CreateShape("NotificationsSubject_Header", model)
                .Location(NotificationConstants.DisplayTypes.Header, "Content:1"));

            if (typeDescriptor != null)
            {
                results.Add(CreateShape("NotificationsType_SummaryAdmin", model)
                    .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                    .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Subject:5")
                    .Location(NotificationConstants.DisplayTypes.Header, "Meta:10")
                    .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Meta:10"));
            }

            results.Add(CreateShape("NotificationsBody_DetailAdmin", model)
                .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1"));
        }

        return Combine(results);
    }

    // Every placed shape needs its own instance.
    private ShapeResult CreateShape(string shapeType, NotificationViewModel model)
        => Factory(shapeType, static (NotificationViewModel m) => new NotificationViewModel(m.Notification)
        {
            IsRead = m.IsRead,
            Visibility = m.Visibility,
            TypeDescriptor = m.TypeDescriptor,
        }, model);
}

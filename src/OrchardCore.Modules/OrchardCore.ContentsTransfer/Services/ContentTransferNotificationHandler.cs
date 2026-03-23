using Microsoft.Extensions.Localization;
using OrchardCore.Notifications;
using OrchardCore.Notifications.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.ContentsTransfer.Services;

public sealed class ContentTransferNotificationHandler : IContentTransferNotificationHandler
{
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;

    internal readonly IStringLocalizer S;

    public ContentTransferNotificationHandler(
        INotificationService notificationService,
        IUserService userService,
        IStringLocalizer<ContentTransferNotificationHandler> stringLocalizer)
    {
        _notificationService = notificationService;
        _userService = userService;
        S = stringLocalizer;
    }

    public async Task NotifyExportCompletedAsync(ContentTransferEntry entry, string contentTypeName)
    {
        if (string.IsNullOrEmpty(entry.Owner))
        {
            return;
        }

        var user = await _userService.GetUserByUniqueIdAsync(entry.Owner);

        if (user == null)
        {
            return;
        }

        var message = new NotificationMessage()
        {
            Subject = S["Content Export Completed"],
            Summary = S["Your export of '{0}' content items is ready for download.", contentTypeName],
            TextBody = S["The export file '{0}' for content type '{1}' has been completed and is ready for download from Bulk Export.", entry.UploadedFileName, contentTypeName],
        };

        await _notificationService.SendAsync(user, message);
    }
}

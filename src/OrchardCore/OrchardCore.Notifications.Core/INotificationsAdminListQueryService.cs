using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Navigation.Core;

public interface INotificationsAdminListQueryService
{
    Task<NotificationQueryResult> QueryAsync(int page, int pageSize, ListNotificationOptions options, IUpdateModel updater);
}

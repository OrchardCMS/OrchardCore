using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Notifications;
using OrchardCore.Notifications.Models;
using YesSql;

namespace OrchardCore.Navigation.Core;

public interface INotificationsAdminListQueryService
{
    Task<IQuery<WebNotification>> QueryAsync(ListNotificationOptions options, IUpdateModel updater);
}

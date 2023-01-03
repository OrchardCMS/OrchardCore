using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Notifications;

public interface INotificationMethodProviderAccessor
{
    Task<IEnumerable<INotificationMethodProvider>> GetProvidersAsync(object notify);
}

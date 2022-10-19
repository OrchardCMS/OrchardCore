using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Users;

namespace OrchardCore.Notifications;

public interface INotificationMethodProviderAccessor
{
    Task<IEnumerable<INotificationMethodProvider>> GetProvidersAsync(IUser user);
}

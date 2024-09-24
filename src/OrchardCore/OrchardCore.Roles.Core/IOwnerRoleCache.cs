
using OrchardCore.Security;

namespace OrchardCore.Roles.Core;

public interface IOwnerRoleCache
{
    ValueTask AddAsync(IRole role);

    ValueTask<IReadOnlySet<string>> GetAsync();

    ValueTask RemoveAsync(IRole role);
}

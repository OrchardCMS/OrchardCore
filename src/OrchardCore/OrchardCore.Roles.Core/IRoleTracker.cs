
using OrchardCore.Security;

namespace OrchardCore.Roles.Core;

public interface IRoleTracker
{
    Task AddAsync(IRole role);

    Task<IReadOnlySet<string>> GetAsync();

    Task RemoveAsync(IRole role);
}

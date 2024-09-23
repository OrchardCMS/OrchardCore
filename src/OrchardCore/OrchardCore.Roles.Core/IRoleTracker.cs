
using OrchardCore.Security;

namespace OrchardCore.Roles.Core;

public interface IRoleTracker
{
    ValueTask AddAsync(IRole role);

    ValueTask<IReadOnlySet<string>> GetAsync();

    ValueTask RemoveAsync(IRole role);
}

using OrchardCore.Roles.Core;
using OrchardCore.Security;

namespace OrchardCore.Tests.Security;

public class RoleTrackerTest : IRoleTracker
{
    private readonly HashSet<string> _roles = new(StringComparer.OrdinalIgnoreCase);

    public RoleTrackerTest()
    {

    }

    public RoleTrackerTest(IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            _roles.Add(role);
        }
    }
    public ValueTask AddAsync(IRole role)
    {
        _roles.Add(role.RoleName);

        return ValueTask.CompletedTask;
    }

    public ValueTask<IReadOnlySet<string>> GetAsync()
    {
        return ValueTask.FromResult<IReadOnlySet<string>>(_roles);
    }

    public ValueTask RemoveAsync(IRole role)
    {
        _roles.Remove(role.RoleName);

        return ValueTask.CompletedTask;
    }
}

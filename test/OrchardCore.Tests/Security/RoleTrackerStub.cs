using OrchardCore.Roles.Core;
using OrchardCore.Security;

namespace OrchardCore.Tests.Security;

public class RoleTrackerStub : IOwnerRoleCache
{
    private readonly HashSet<string> _ownerRoles = new(StringComparer.OrdinalIgnoreCase);

    public RoleTrackerStub()
    {
    }

    public RoleTrackerStub(IEnumerable<string> roles)
    {
        foreach (var role in roles)
        {
            _ownerRoles.Add(role);
        }
    }
    public ValueTask AddAsync(IRole role)
    {
        _ownerRoles.Add(role.RoleName);

        return ValueTask.CompletedTask;
    }

    public ValueTask<IReadOnlySet<string>> GetAsync()
    {
        return ValueTask.FromResult<IReadOnlySet<string>>(_ownerRoles);
    }

    public ValueTask RemoveAsync(IRole role)
    {
        _ownerRoles.Remove(role.RoleName);

        return ValueTask.CompletedTask;
    }
}

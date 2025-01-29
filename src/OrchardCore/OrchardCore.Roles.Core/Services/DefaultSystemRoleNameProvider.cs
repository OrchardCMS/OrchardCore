using System.Collections.Frozen;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Roles;

#pragma warning disable CS0618 // Type or member is obsolete
internal sealed class DefaultSystemRoleNameProvider : ISystemRoleNameProvider
#pragma warning restore CS0618 // Type or member is obsolete
{
    private readonly ISystemRoleProvider _provider;

    private readonly FrozenSet<string> _systemRoleNames;

    public DefaultSystemRoleNameProvider(ISystemRoleProvider provider)
    {
        _provider = provider;
        _systemRoleNames = provider.GetSystemRoles().Select(x => x.RoleName).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    public ValueTask<string> GetAdminRoleAsync()
        => ValueTask.FromResult(_provider.GetAdminRole().RoleName);

    public ValueTask<FrozenSet<string>> GetSystemRolesAsync()
        => ValueTask.FromResult(_systemRoleNames);
}

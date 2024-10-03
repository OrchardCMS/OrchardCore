using System.Collections.Frozen;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Roles;

internal sealed class DefaultSystemRoleNameProvider : ISystemRoleNameProvider
{
    private readonly string _adminRoleName;

    private readonly FrozenSet<string> _systemRoleNames;

    public DefaultSystemRoleNameProvider(
        ShellSettings shellSettings,
        IOptions<SystemRoleOptions> options)
    {
        _adminRoleName = shellSettings["AdminRoleName"];

        if (string.IsNullOrWhiteSpace(_adminRoleName))
        {
            _adminRoleName = options.Value.SystemAdminRoleName;
        }

        if (string.IsNullOrWhiteSpace(_adminRoleName))
        {
            _adminRoleName = OrchardCoreConstants.Roles.Administrator;
        }

        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            OrchardCoreConstants.Roles.Anonymous,
            OrchardCoreConstants.Roles.Authenticated,
            _adminRoleName,
        };

        _systemRoleNames = roles.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    public ValueTask<string> GetAdminRoleAsync()
        => ValueTask.FromResult(_adminRoleName);

    public ValueTask<FrozenSet<string>> GetSystemRolesAsync()
        => ValueTask.FromResult(_systemRoleNames);
}

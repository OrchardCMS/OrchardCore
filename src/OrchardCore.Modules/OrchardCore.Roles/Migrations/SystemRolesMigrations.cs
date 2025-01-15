using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Security;

namespace OrchardCore.Roles.Migrations;

public sealed class SystemRolesMigrations : DataMigration
{
    private readonly ISystemRoleProvider _systemRoleProvider;
    private readonly RoleManager<IRole> _roleManager;
    private readonly ILogger _logger;

    public SystemRolesMigrations(
        ISystemRoleProvider systemRoleProvider,
        RoleManager<IRole> roleManager,
        ILogger<RolesMigrations> logger)
    {
        _systemRoleProvider = systemRoleProvider;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<int> CreateAsync()
    {
        var systemRoles = await _systemRoleProvider.GetSystemRolesAsync();

        foreach (var systemRole in systemRoles)
        {
            var role = await _roleManager.FindByNameAsync(systemRole.RoleName);

            if (role is null)
            {
                await _roleManager.CreateAsync(role);
            }
        }

        _logger.LogInformation("The system roles have been created successfully.");

        return 1;
    }
}

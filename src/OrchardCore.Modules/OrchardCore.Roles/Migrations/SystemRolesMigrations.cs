using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Data.Migration;
using OrchardCore.Security;

namespace OrchardCore.Roles.Migrations;

public sealed class SystemRolesMigrations : DataMigration
{
    private readonly ISystemRoleNameProvider _systemRoleNameProvider;
    private readonly RoleManager<IRole> _roleManager;
    private readonly ILogger _logger;

    public SystemRolesMigrations(
        ISystemRoleNameProvider systemRoleNameProvider,
        RoleManager<IRole> roleManager,
        ILogger<RolesMigrations> logger)
    {
        _systemRoleNameProvider = systemRoleNameProvider;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<int> CreateAsync()
    {
        var systemRoles = await _systemRoleNameProvider.GetSystemRolesAsync();

        foreach (var role in systemRoles)
        {
            if (await _roleManager.FindByNameAsync(role) is null)
            {
                await _roleManager.CreateAsync(new Role { RoleName = role });
            }
        }

        _logger.LogInformation("The system roles have been created successfully.");

        return 1;
    }
}

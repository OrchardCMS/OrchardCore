using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Infrastructure.Security;
using OrchardCore.Security;

namespace OrchardCore.Roles.Migrations;

public sealed class RolesMigrations : DataMigration
{
#pragma warning disable CA1822 // Mark members as static
    public int Create()
#pragma warning restore CA1822 // Mark members as static
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IRole>>();

            await UpdateSystemRoleAsync(roleManager, OrchardCoreConstants.Roles.Anonymous);
            await UpdateSystemRoleAsync(roleManager, OrchardCoreConstants.Roles.Authenticated);

            var roles = roleManager.Roles.ToList();

            foreach (var role in roles)
            {
                if (role.Type == RoleType.Owner || role is not Role r)
                {
                    continue;
                }

                if (r.RoleClaims.Any(x => x.ClaimValue == "SiteOwner"))
                {
                    r.Type = RoleType.Owner;

                    await roleManager.UpdateAsync(r);
                }
            }
        });

        return 1;
    }

    private static async Task UpdateSystemRoleAsync(RoleManager<IRole> roleManager, string name)
    {
        var role = await roleManager.FindByNameAsync(roleManager.NormalizeKey(name));

        if (role is not null && role.Type is not RoleType.System)
        {
            role.Type = RoleType.System;

            await roleManager.UpdateAsync(role);
        }
    }
}

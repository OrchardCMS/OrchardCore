using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Security;
using OrchardCore.Users;

namespace OrchardCore.Roles.Migrations;

public sealed class RolesMigrations : DataMigration
{
    private static readonly string[] _alternativeAdminRoles =
    [
        "Admin",
        "SiteAdmin",
        "SiteAdministrator",
        "SiteOwner",
    ];

#pragma warning disable CA1822 // Mark members as static
    public int Create()
#pragma warning restore CA1822 // Mark members as static
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IRole>>();

            var roles = roleManager.Roles.ToList();

            var adminRoles = new List<Role>();
            var adminSystemRoleName = OrchardCoreConstants.Roles.Administrator;

            foreach (var role in roles)
            {
                if (role is not Role r)
                {
                    continue;
                }

                var hasSiteOwner = r.RoleClaims.Any(x => x.ClaimValue == "SiteOwner");

                if (r.RoleName.Equals(OrchardCoreConstants.Roles.Administrator, StringComparison.OrdinalIgnoreCase))
                {
                    if (!hasSiteOwner)
                    {
                        // At this point, the tenant is using the Administrator role without 'SiteOwner' permission.
                        // We'll need to create a new role name that does not exists and assign it as the system 'Administrator' role.
                        adminSystemRoleName = GenerateNewAdminRoleName(roles);

                        await roleManager.CreateAsync(new Role
                        {
                            RoleName = adminSystemRoleName,
                        });
                    }
                    else
                    {
                        r.RoleClaims.Clear();

                        await roleManager.UpdateAsync(r);
                    }

                    continue;
                }

                if (hasSiteOwner)
                {
                    adminRoles.Add(r);
                }
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

            foreach (var adminRole in adminRoles)
            {
                var users = await userManager.GetUsersInRoleAsync(adminRole.RoleName);

                foreach (var user in users)
                {
                    await userManager.AddToRoleAsync(user, adminSystemRoleName);
                }
            }

            if (adminSystemRoleName != OrchardCoreConstants.Roles.Administrator)
            {
                // At this point, we'll update the shell settings and release the shell.
                var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
                var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();

                shellSettings.SetSystemAdminRoleName(adminSystemRoleName);

                await shellHost.UpdateShellSettingsAsync(shellSettings);
                await shellHost.ReleaseShellContextAsync(shellSettings);
            }
        });

        return 1;
    }

    private static string GenerateNewAdminRoleName(List<IRole> roles)
    {
        foreach (var alternativeAdminRole in _alternativeAdminRoles)
        {
            if (!RoleExists(roles, alternativeAdminRole))
            {
                return alternativeAdminRole;
            }
        }

        var counter = 1;
        string roleName;
        do
        {
            // Generate names like this Admin1, Admin2....Admin{N}
            roleName = _alternativeAdminRoles[0] + counter++;
        }
        while (RoleExists(roles, roleName));

        return roleName;
    }

    private static bool RoleExists(List<IRole> roles, string roleName)
        => roles.Any(role => role.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
}

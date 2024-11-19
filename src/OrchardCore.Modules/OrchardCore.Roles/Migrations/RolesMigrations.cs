using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundJobs;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Security;
using OrchardCore.Users;

namespace OrchardCore.Roles.Migrations;

public sealed class RolesMigrations : DataMigration
{
    private static readonly string _alternativeAdminRoleName = "SiteOwner";

    private readonly SystemRoleOptions _systemRoleOptions;

    public RolesMigrations(IOptions<SystemRoleOptions> systemRoleOptions)
    {
        _systemRoleOptions = systemRoleOptions.Value;
    }

#pragma warning disable CA1822 // Mark members as static
    public int Create()
#pragma warning restore CA1822 // Mark members as static
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IRole>>();

            var roles = roleManager.Roles.ToList();

            var adminRoles = new List<Role>();
            var adminSystemRoleName = _systemRoleOptions.SystemAdminRoleName;

            foreach (var role in roles)
            {
                if (role is not Role r)
                {
                    continue;
                }

                // When a new tenant is created, the RoleClaims will be empty for Admin roles.
                var hasSiteOwner = r.RoleClaims is null ||
                r.RoleClaims.Count == 0 ||
                r.RoleClaims.Any(x => x.ClaimValue == "SiteOwner");

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
                }

                if (hasSiteOwner)
                {
                    adminRoles.Add(r);
                }
            }

            if (adminRoles.Count > 0)
            {
                // Run the migration in the background to ensure that the newly added role is committed to the database first, preventing potential exceptions.
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("MigrateAdminUsersToNewAdminRole", async subScope =>
                {
                    var userManager = subScope.ServiceProvider.GetRequiredService<UserManager<IUser>>();

                    foreach (var adminRole in adminRoles)
                    {
                        var users = await userManager.GetUsersInRoleAsync(adminRole.RoleName);

                        foreach (var user in users)
                        {
                            await userManager.AddToRoleAsync(user, adminSystemRoleName);
                        }
                    }
                });
            }

            if (adminSystemRoleName != OrchardCoreConstants.Roles.Administrator)
            {
                // At this point, we'll update the shell settings and release the shell.
                var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
                var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();

                shellSettings["AdminRoleName"] = adminSystemRoleName;

                await shellHost.UpdateShellSettingsAsync(shellSettings);
                await shellHost.ReleaseShellContextAsync(shellSettings);
            }
        });

        return 1;
    }

    private static string GenerateNewAdminRoleName(List<IRole> roles)
    {
        var counter = 1;
        var roleName = _alternativeAdminRoleName;

        while (RoleExists(roles, roleName))
        {
            // Generate names like this SiteOwner1, SiteOwner2...SiteOwner{N}
            roleName = _alternativeAdminRoleName + counter++;
        }

        return roleName;
    }

    private static bool RoleExists(List<IRole> roles, string roleName)
        => roles.Any(role => role.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
}

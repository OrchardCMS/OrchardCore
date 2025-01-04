using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger _logger;

    public RolesMigrations(
        IOptions<SystemRoleOptions> systemRoleOptions,
        ILogger<RolesMigrations> logger)
    {
        _systemRoleOptions = systemRoleOptions.Value;
        _logger = logger;
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

                // Check to see if the role contains the obsolete 'SiteOwner' permission claim.
                var hasSiteOwner = r.RoleClaims is not null && r.RoleClaims.Any(x => x.ClaimValue == "SiteOwner");

                if (r.RoleName.Equals(OrchardCoreConstants.Roles.Administrator, StringComparison.OrdinalIgnoreCase))
                {
                    if (!hasSiteOwner)
                    {
                        // At this point, the tenant is using the Administrator role without 'SiteOwner' permission.
                        // We'll need to create a new role name that does not exists and assign it as the system 'Administrator' role.
                        adminSystemRoleName = GenerateNewAdminRoleName(roles);

                        _logger.LogInformation("The {DefaultAdministratorRoleName} does not contain SiteOwner permission. Creating a new AdminRoleName as the system admin name. The new role name is {NewAdminRoleName}.", OrchardCoreConstants.Roles.Administrator, adminSystemRoleName);

                        await roleManager.CreateAsync(new Role
                        {
                            RoleName = adminSystemRoleName,
                        });

                    }
                    else
                    {
                        _logger.LogInformation("Removing all existing permission claims from the default {DefaultAdministratorRoleName} Administrator name.", OrchardCoreConstants.Roles.Administrator);

                        r.RoleClaims.Clear();

                        await roleManager.UpdateAsync(r);

                        // Don't processed to avoid adding the default 'Administrator' role to the adminRoles collection;
                        continue;
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

                        if (users.Count > 0)
                        {
                            _logger.LogInformation("Migrating all users {Count} users from {PreviousRoleName} to {NewRoleName}", users.Count, adminRole, adminSystemRoleName);

                            foreach (var user in users)
                            {
                                await userManager.AddToRoleAsync(user, adminSystemRoleName);
                            }
                        }
                    }
                });
            }

            if (adminSystemRoleName != OrchardCoreConstants.Roles.Administrator)
            {
                // At this point, we'll update the shell settings and release the shell.
                var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();
                var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();

                _logger.LogInformation("The {DefaultAdministratorRoleName} does not contain SiteOwner permission. Creating a new AdminRoleName as the system admin name and storing it in the tenant app settings provider. The new name is {NewAdminRoleName}", OrchardCoreConstants.Roles.Administrator, adminSystemRoleName);

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

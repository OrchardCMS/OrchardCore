using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users.DataMigrations;

public sealed class AssignRoleToUsersMigrations : DataMigration
{
#pragma warning disable CA1822 // Mark members as static
    public int Create()
#pragma warning restore CA1822 // Mark members as static
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IRole>>();

            foreach (var role in roleManager.Roles.ToList())
            {
                if (role is not Role r)
                {
                    continue;
                }

                var permissions = r.RoleClaims.Where(x => x.ClaimType == Permission.ClaimType).ToArray();

                if (permissions.Any(x => x.ClaimValue == "AssignRoles") &&
                !permissions.Any(x => x.ClaimValue == CommonPermissions.AssignRoleToUsers.Name))
                {
                    r.RoleClaims.Add(new RoleClaim()
                    {
                        ClaimType = Permission.ClaimType,
                        ClaimValue = CommonPermissions.AssignRoleToUsers.Name,
                    });
                }

                var roleClaimValue = CommonPermissions.CreateAssignRoleToUsersPermission(r.RoleName).Name;

                if (permissions.Any(x => x.ClaimValue == $"AssignRole_{r.RoleName}") &&
                !permissions.Any(x => x.ClaimValue == roleClaimValue))
                {
                    r.RoleClaims.Add(new RoleClaim()
                    {
                        ClaimType = Permission.ClaimType,
                        ClaimValue = roleClaimValue,
                    });
                }

                await roleManager.UpdateAsync(r);
            }
        });

        return 1;
    }
}

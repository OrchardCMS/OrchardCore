using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security;

namespace Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    private static readonly IRole[] _systemRoles =
    [
        new Role
        {
            RoleName = "Administrator",
            RoleDescription = "A system role that grants all permissions to the assigned users."
        },
        new Role
        {
            RoleName = "Authenticated",
            RoleDescription = "A system role representing all authenticated users."
        },
        new Role
        {
            RoleName = "Anonymous",
            RoleDescription = "A system role representing all non-authenticated users."
        },
        new Role
        {
            RoleName = "Moderator",
            RoleDescription = "Grants users the ability to moderate content."
        },
        new Role
        {
            RoleName = "Author",
            RoleDescription = "Grants users the ability to create content."
        },
        new Role
        {
            RoleName = "Contributor",
            RoleDescription = "Grants users the ability to contribute content."
        }
    ];

    public static IApplicationBuilder UseSystemRoles(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<IRole>>();

        foreach (var role in _systemRoles)
        {
            roleManager.CreateAsync(role);
        }

        return app;
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security.Services;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.Users.Services;

public class RolesAdminListFilterProvider : IUsersAdminListFilterProvider
{
    public void Build(QueryEngineBuilder<User> builder)
    {
        builder.WithNamedTerm("role-restriction", builder => builder
                    .OneCondition(async (contentType, query, ctx) =>
                    {
                        var context = (UserQueryContext)ctx;

                        var httpContextAccessor = context.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
                        var authorizationService = context.ServiceProvider.GetRequiredService<IAuthorizationService>();
                        var roleService = context.ServiceProvider.GetRequiredService<IRoleService>();

                        var user = httpContextAccessor.HttpContext?.User;

                        if (user != null && !await authorizationService.AuthorizeAsync(user, CommonPermissions.ListUsers))
                        {
                            // At this point the user cannot see all users, so lets see what role does he have access too and filter by them.
                            var accessibleRoles = await roleService.GetAccessibleRoleNamesAsync(authorizationService, user, CommonPermissions.ListUsers);

                            query.With<UserByRoleNameIndex>(index => index.RoleName.IsIn(accessibleRoles));
                        }

                        return query;
                    })
                    .AlwaysRun()
                );
    }
}

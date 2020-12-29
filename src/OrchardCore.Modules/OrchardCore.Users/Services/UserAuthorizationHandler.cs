using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// This provides authorization when we request ManageUsers and the supplied user resource has permission
    /// to edit all of the roles the current user resource has permission for.
    /// Callers should supply a <see cref="IUser"/> resource
    /// </summary>
    /// <remarks>
    /// This handler will not be triggered when only requesting ManagerUsers permission.
    /// </remarks>
    public class UserAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserManager<IUser> _userManager;

        public UserAuthorizationHandler(IServiceProvider serviceProvider, UserManager<IUser> userManager)
        {
            _serviceProvider = serviceProvider;
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.HasSucceeded)
            {
                // This handler is not revoking any pre-existing grants.
                return;
            }

            if (context.Resource == null)
            {
                return;
            }

            if (!String.Equals(requirement.Permission.Name, Permissions.ManageUsers.Name, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var user = context.Resource as IUser;

            if (user == null)
            {
                return;
            }

            // Lazy load to prevent circular dependencies
            var authorizationService = _serviceProvider.GetService<IAuthorizationService>();

            var userRoleNames = await _userManager.GetRolesAsync(user);
            if (!userRoleNames.Any())
            {
                // When the user has no roles we are able to check if the current user can manage authenticated roles.
                if (await authorizationService.AuthorizeAsync(context.User, CommonPermissions.ManageUsersInAuthenticatedRole))
                {
                    context.Succeed(requirement);
                }
            }
            else
            {
                // When the user has roles we check to see if any of these roles are not authorized before succeeding.
                if (await AuthorizeRolesAsync(authorizationService, context.User, userRoleNames))
                {
                    context.Succeed(requirement);
                }
            }
        }

        private async Task<bool> AuthorizeRolesAsync(IAuthorizationService authorizationService, ClaimsPrincipal user, IEnumerable<string> roleNames)
        {
            foreach (var roleName in roleNames)
            {
                if (!await authorizationService.AuthorizeAsync(user, CommonPermissions.CreatePermissionForManageUsersInRole(roleName)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

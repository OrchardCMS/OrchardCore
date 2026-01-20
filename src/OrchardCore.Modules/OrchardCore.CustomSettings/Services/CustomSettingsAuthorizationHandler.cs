using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.CustomSettings.Services
{
    public class CustomSettingsAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomSettingsAuthorizationHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.HasSucceeded)
            {
                // This handler is not revoking any pre-existing grants.
                return;
            }

            if (requirement.Permission.Name != "ManageResourceSettings")
            {
                return;
            }

            if (context.Resource == null || context.Resource.ToString() == "")
            {
                return;
            }

            // Lazy load to prevent circular dependencies
            var authorizationService = _serviceProvider.GetService<IAuthorizationService>();

            if (await authorizationService.AuthorizeAsync(context.User, new Permission(Permissions.CreatePermissionName(context.Resource.ToString()))))
            {
                context.Succeed(requirement);
            }
        }
    }
}

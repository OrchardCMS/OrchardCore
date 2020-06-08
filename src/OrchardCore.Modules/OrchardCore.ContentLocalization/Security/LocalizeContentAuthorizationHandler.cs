using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentLocalization.Security
{
    public class LocalizeContentAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public LocalizeContentAuthorizationHandler(IServiceProvider serviceProvider)
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

            if (context.Resource == null)
            {
                return;
            }

            var contentItem = context.Resource as ContentItem;

            Permission permission = null;

            if (contentItem != null)
            {
                if (OwnerVariationExists(requirement.Permission) && HasOwnership(context.User, contentItem))
                {
                    permission = GetOwnerVariation(requirement.Permission);
                }
            }

            if (permission == null)
            {
                return;
            }

            // Lazy load to prevent circular dependencies
            var authorizationService = _serviceProvider.GetService<IAuthorizationService>();

            if (await authorizationService.AuthorizeAsync(context.User, permission))
            {
                context.Succeed(requirement);
            }
        }

        private static Permission GetOwnerVariation(Permission permission)
        {
            if (permission.Name == Permissions.LocalizeContent.Name)
            {
                return Permissions.LocalizeOwnContent;
            }

            return null;
        }

        private static bool HasOwnership(ClaimsPrincipal user, ContentItem content)
        {
            if (user == null || content == null)
            {
                return false;
            }

            return user.Identity.Name == content.Owner;
        }

        private static bool OwnerVariationExists(Permission permission)
        {
            return GetOwnerVariation(permission) != null;
        }
    }
}

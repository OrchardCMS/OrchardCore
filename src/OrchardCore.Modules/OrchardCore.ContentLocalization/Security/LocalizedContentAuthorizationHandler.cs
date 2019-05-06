using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement;
using OrchardCore.Security;

namespace OrchardCore.ContentLocalization.Security
{
    public class LocalizedContentAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.HasSucceeded)
            {
                // This handler is not revoking any pre-existing grants.
                return Task.CompletedTask;
            }

            if (context.Resource == null)
            {
                return Task.CompletedTask;
            }

            var contentItem = context.Resource as ContentItem;

            if (contentItem != null)
            {
                if (HasOwnership(context.User, contentItem))
                {
                    requirement.Permission = Permissions.EditOwnLocalizedContent;
                }
            }

            return Task.CompletedTask;
        }

        private static bool HasOwnership(ClaimsPrincipal user, ContentItem content)
        {
            if (user == null || content == null)
            {
                return false;
            }

            return user.Identity.Name == content.Owner;
        }
    }
}

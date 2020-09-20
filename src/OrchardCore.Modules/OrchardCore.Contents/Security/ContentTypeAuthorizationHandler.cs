using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Core;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents.Security
{
    public class ContentTypeAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentTypeAuthorizationHandler(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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

            if (await ContentTypeAuthorizationHelper.AuthorizeDynamicPermissionAsync(_httpContextAccessor.HttpContext, requirement.Permission, contentItem))
            {
                context.Succeed(requirement);
            }
        }

        private static Permission GetOwnerVariation(Permission permission)
        {
            if (permission.Name == Permissions.PublishContent.Name)
            {
                return Permissions.PublishOwnContent;
            }

            if (permission.Name == Permissions.EditContent.Name)
            {
                return Permissions.EditOwnContent;
            }

            if (permission.Name == Permissions.DeleteContent.Name)
            {
                return Permissions.DeleteOwnContent;
            }

            if (permission.Name == Permissions.ViewContent.Name)
            {
                return Permissions.ViewOwnContent;
            }

            if (permission.Name == Permissions.PreviewContent.Name)
            {
                return Permissions.PreviewOwnContent;
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

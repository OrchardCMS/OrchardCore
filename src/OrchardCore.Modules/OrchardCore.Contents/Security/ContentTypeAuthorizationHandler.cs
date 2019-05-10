using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents.Security
{
    public class ContentTypeAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
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
                if (OwnerVariationExists(requirement.Permission) && HasOwnership(context.User, contentItem))
                {
                    requirement.Permission = GetOwnerVariation(requirement.Permission);
                }
            }

            var contentTypePermission = ContentTypePermissions.ConvertToDynamicPermission(requirement.Permission);

            if (contentTypePermission == null)
            {
                return Task.CompletedTask;
            }

            // The resource can be a content type name
            var contentType = contentItem != null
                ? contentItem.ContentType
                : Convert.ToString(context.Resource.ToString())
                ;

            if (String.IsNullOrEmpty(contentType))
            {
                return Task.CompletedTask;
            }

            requirement.Permission = ContentTypePermissions.CreateDynamicPermission(contentTypePermission, contentType);

            return Task.CompletedTask;
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

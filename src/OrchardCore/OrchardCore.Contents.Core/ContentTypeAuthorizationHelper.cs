using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents.Core
{
    public static class ContentTypeAuthorizationHelper
    {
        public static Task<bool> AuthorizeDynamicPermissionAsync(HttpContext context, Permission requirementPermission, ContentItem contentItem)
        {
            Permission permission = null;

            if (contentItem != null)
            {
                if (OwnerVariationExists(requirementPermission) && HasOwnership(context.User, contentItem))
                {
                    permission = GetOwnerVariation(requirementPermission);
                }
            }
            else
            {
                return Task.FromResult(false);
            }

            var contentTypePermission = ContentTypePermissionsHelper.ConvertToDynamicPermission(permission ?? requirementPermission);

            if (contentTypePermission != null)
            {
                if (!String.IsNullOrEmpty(contentItem.ContentType))
                {
                    permission = ContentTypePermissionsHelper.CreateDynamicPermission(contentTypePermission, contentItem.ContentType);
                }
            }

            var authorizationService = context.RequestServices.GetService<IAuthorizationService>();

            return authorizationService.AuthorizeAsync(context.User, permission);
        }

        private static Permission GetOwnerVariation(Permission permission)
        {
            if (permission.Name == CommonPermissions.PublishContent.Name)
            {
                return CommonPermissions.PublishOwnContent;
            }

            if (permission.Name == CommonPermissions.EditContent.Name)
            {
                return CommonPermissions.EditOwnContent;
            }

            if (permission.Name == CommonPermissions.DeleteContent.Name)
            {
                return CommonPermissions.DeleteOwnContent;
            }

            if (permission.Name == CommonPermissions.ViewContent.Name)
            {
                return CommonPermissions.ViewOwnContent;
            }

            if (permission.Name == CommonPermissions.PreviewContent.Name)
            {
                return CommonPermissions.PreviewOwnContent;
            }

            if (permission.Name == CommonPermissions.CloneContent.Name)
            {
                return CommonPermissions.CloneOwnContent;
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

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Security.Permissions;

namespace Microsoft.AspNetCore.Authorization
{
    public static class AuthorizationServiceExtensions
    {
        public static Task<bool> AuthorizeContentTypeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission requiredPermission, ContentTypeDefinition contentTypeDefinition, string owner = null)
        {
            if (contentTypeDefinition == null)
            {
                throw new ArgumentNullException(nameof(contentTypeDefinition));
            }

            return service.AuthorizeContentTypeAsync(user, requiredPermission, contentTypeDefinition.Name, owner);
        }

        public static Task<bool> AuthorizeContentTypeAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission requiredPermission, string contentType, string owner = null)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (requiredPermission == null)
            {
                throw new ArgumentNullException(nameof(requiredPermission));
            }

            if (String.IsNullOrWhiteSpace(contentType))
            {
                throw new ArgumentException($"{nameof(contentType)} cannot be empty.");
            }

            var item = new ContentItem()
            {
                ContentType = contentType,
                Owner = owner,
            };

            return service.AuthorizeAsync(user, requiredPermission, item);
        }

        /// <summary>
        /// Evaluate if we have a specific owner variation permission to at least one content type
        /// </summary>
        public static async Task<bool> AuthorizeContentTypeDefinitionsAsync(this IAuthorizationService service, ClaimsPrincipal user, Permission requiredPermission, IEnumerable<ContentTypeDefinition> contentTypeDefinitions, IContentManager contentManager)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(requiredPermission);
            ArgumentNullException.ThrowIfNull(contentManager);

            var permission = GetOwnerVariation(requiredPermission) ?? requiredPermission;

            var contentTypePermission = ContentTypePermissionsHelper.ConvertToDynamicPermission(permission);

            foreach (var contentTypeDefinition in contentTypeDefinitions)
            {
                var dynamicPermission = ContentTypePermissionsHelper.CreateDynamicPermission(contentTypePermission, contentTypeDefinition);

                var contentItem = await contentManager.NewAsync(contentTypeDefinition.Name);
                contentItem.Owner = user.FindFirstValue(ClaimTypes.NameIdentifier);

                if (await service.AuthorizeAsync(user, dynamicPermission, contentItem))
                {
                    return true;
                }
            }

            return await service.AuthorizeAsync(user, permission);
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
    }
}

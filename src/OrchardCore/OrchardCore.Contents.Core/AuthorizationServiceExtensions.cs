using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace Microsoft.AspNetCore.Authorization
{
    public static class AuthorizationServiceExtensions
    {
        /// <summary>
        /// Evaluate if we have a specific owner variation permission to at least one content type
        /// </summary>
        public static async Task<bool> IsAuthorizedToAContentTypeAsync(this IAuthorizationService service, Permission requiredPermission, IEnumerable<ContentTypeDefinition> contentTypeDefinitions, HttpContext context)
        {
            foreach(var contentTypeDefinition in contentTypeDefinitions)
            {
                var permission = GetOwnerVariation(requiredPermission);
                var contentTypePermission = ContentTypePermissionsHelper.ConvertToDynamicPermission(permission);
                permission = ContentTypePermissionsHelper.CreateDynamicPermission(contentTypePermission, contentTypeDefinition);
                
                if(await service.AuthorizeAsync(context.User, permission, new ContentItem{ ContentType = contentTypeDefinition.Name, Owner = context.User.Identity.Name }))
                {
                    return true;
                }
            }

            return false;
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

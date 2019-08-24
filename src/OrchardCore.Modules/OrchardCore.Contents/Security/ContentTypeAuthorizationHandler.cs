using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents.Security
{
    public class ContentTypeAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceProvider _serviceProvider;

        public ContentTypeAuthorizationHandler(IServiceProvider serviceProvider)
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

            var contentTypePermission = ContentTypePermissions.ConvertToDynamicPermission(permission ?? requirement.Permission);

            if (contentTypePermission != null)
            {
                // The resource can be a content type name
                var contentType = contentItem != null
                    ? contentItem.ContentType
                    : Convert.ToString(context.Resource.ToString())
                    ;

                if (!String.IsNullOrEmpty(contentType))
                {
                    permission = ContentTypePermissions.CreateDynamicPermission(contentTypePermission, contentType);
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

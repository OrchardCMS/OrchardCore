using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement;
using static OrchardCore.Contents.CommonPermissions;
using OrchardCore.Security;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Title.Security
{
    public class EditTitleAuthorizationEventHandler : AuthorizationHandler<PermissionRequirement>    
    {
        private readonly IServiceProvider _serviceProvider;

        public EditTitleAuthorizationEventHandler(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var authorizationService = _serviceProvider.GetService<IAuthorizationService>();

            if (requirement.Permission.Name != Permissions.EditTitlePart.Name) return;

            var contentItem = context.Resource as ContentItem;
            if (contentItem == null)  return;

            if (!await authorizationService.AuthorizeAsync(context.User, EditContent, context.Resource))
            {
                context.Fail();
            }
        }
    }
}

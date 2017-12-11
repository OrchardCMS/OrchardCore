using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents
{
    public static class ContentItemExtensions
    {
        public static async Task<IEnumerable<ContentItem>> FilterByRole(
            this IEnumerable<ContentItem> contentItems,
            IAuthorizationService authorizationService,
            Permission permission,
            ClaimsPrincipal user)
        {
            var contentItemAuthorization = contentItems.Select(ci => new {
                ContentItem = ci,
                IsAuthorizedTask = authorizationService.AuthorizeAsync(user, Permissions.ViewContent, ci)
            });

            await Task.WhenAll(contentItemAuthorization.Select(x => x.IsAuthorizedTask));

            return contentItemAuthorization
                .Where(x => x.IsAuthorizedTask.Result)
                .Select(x => x.ContentItem)
                .ToArray();
        }
    }
}

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public interface IContentQueryService
    {
        Task<IEnumerable<ContentTypeDefinition>> GetListableTypesAsync(ClaimsPrincipal user);
        Task<IQuery<ContentItem, ContentItemIndex>> GetQueryByOptions(ViewModels.ContentOptions options);
    }
}

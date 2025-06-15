using OrchardCore.ContentManagement;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Models;
using YesSql;

namespace OrchardCore.Taxonomies.Core;

public interface IContentsTaxonomyListQueryService
{
    Task<IQuery<ContentItem>> QueryAsync(TermPart termPart, Pager pager);
}

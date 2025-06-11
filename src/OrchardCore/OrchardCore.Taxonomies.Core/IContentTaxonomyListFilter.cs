using OrchardCore.ContentManagement;
using YesSql;

namespace OrchardCore.Taxonomies.Core;

public interface IContentTaxonomyListFilter
{
    Task FilterAsync(IQuery<ContentItem> query);
}

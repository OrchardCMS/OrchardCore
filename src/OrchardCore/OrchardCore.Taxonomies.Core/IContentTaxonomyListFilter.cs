using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Models;
using YesSql;

namespace OrchardCore.Taxonomies.Core;

public interface IContentTaxonomyListFilter
{
    Task FilterAsync(IQuery<ContentItem> query, TermPart termPart);
}

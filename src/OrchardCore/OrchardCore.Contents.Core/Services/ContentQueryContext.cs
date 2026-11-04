using OrchardCore.ContentManagement;
using YesSql;
using YesSql.Filters.Query.Services;

namespace OrchardCore.Contents.Services;

public class ContentQueryContext : QueryExecutionContext<ContentItem>
{
    public ContentQueryContext(IServiceProvider serviceProvider, IQuery<ContentItem> query) : base(query)
    {
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets or sets whether the <c>sort</c> term ordered the query by the modified date, which an index can serve.
    /// </summary>
    public bool OrderedByModified { get; set; }
}

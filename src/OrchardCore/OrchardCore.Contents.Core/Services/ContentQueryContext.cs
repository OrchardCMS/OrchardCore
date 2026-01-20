using System;
using OrchardCore.ContentManagement;
using YesSql;
using YesSql.Filters.Query.Services;

namespace OrchardCore.Contents.Services
{
    public class ContentQueryContext : QueryExecutionContext<ContentItem>
    {
        public ContentQueryContext(IServiceProvider serviceProvider, IQuery<ContentItem> query) : base(query)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }
    }
}

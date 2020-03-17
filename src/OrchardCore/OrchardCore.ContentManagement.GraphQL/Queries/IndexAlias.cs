using System;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public class IndexAlias
    {
        public string Alias { get; set; }

        public string Index { get; set; }

        public Func<IQuery<ContentItem>, IQuery<ContentItem>> With { get; set; }
    }
}

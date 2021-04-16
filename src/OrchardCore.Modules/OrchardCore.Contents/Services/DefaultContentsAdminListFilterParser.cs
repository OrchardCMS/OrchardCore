using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.Filters.Query;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListFilterParser : IContentsAdminListFilterParser
    {
        private readonly IQueryParser<ContentItem> _parser;

        public DefaultContentsAdminListFilterParser(IEnumerable<IContentsAdminListFilterProvider> providers)
        {
            // TODO build lazily, and resolve/relase the providers
            var builder = new QueryEngineBuilder<ContentItem>();
            foreach (var provider in providers)
            {
                provider.Build(builder);
            }

            _parser = builder.Build();
        }

        public QueryFilterResult<ContentItem> Parse(string text)
            => _parser.Parse(text);
    }
}

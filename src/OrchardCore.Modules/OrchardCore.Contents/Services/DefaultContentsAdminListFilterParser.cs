using OrchardCore.ContentManagement;
using YesSql.Filters.Query;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListFilterParser : IContentsAdminListFilterParser
    {
        private readonly IQueryParser<ContentItem> _parser;

        public DefaultContentsAdminListFilterParser(IQueryParser<ContentItem> parser)
        {
            _parser = parser;
        }

        public QueryFilterResult<ContentItem> Parse(string text)
            => _parser.Parse(text);
    }
}

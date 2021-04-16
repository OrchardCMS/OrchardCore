using OrchardCore.ContentManagement;
using OrchardCore.Filters.Abstractions.Services;
using OrchardCore.Filters.Query;
using OrchardCore.Filters.Query.Services;

namespace OrchardCore.Contents.Services
{
    public class ContentItemFilterEngineModelBinder : FilterEngineModelBinder<QueryFilterResult<ContentItem>>
    {
        private readonly IContentsAdminListFilterParser _parser;

        public ContentItemFilterEngineModelBinder(IContentsAdminListFilterParser parser)
        {
            _parser = parser;
        }

        public override QueryFilterResult<ContentItem> Parse(string text)
            => _parser.Parse(text);
    }
}
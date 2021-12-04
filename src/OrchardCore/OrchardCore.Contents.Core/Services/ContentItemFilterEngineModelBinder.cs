using OrchardCore.ContentManagement;
using OrchardCore.Filters.Core;
using YesSql.Filters.Query;

namespace OrchardCore.Contents.Services
{
    public class ContentItemFilterEngineModelBinder : FilterEngineModelBinder<ContentItem>
    {
        public ContentItemFilterEngineModelBinder(IQueryParser<ContentItem> parser) : base(parser)
        {
        }
    }
}

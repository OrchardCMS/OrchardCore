using OrchardCore.ContentManagement;
using OrchardCore.Filters.Core;

namespace OrchardCore.Contents.Services
{
    public class ContentItemFilterEngineModelBinder : FilterEngineModelBinder<ContentItem>
    {
        public ContentItemFilterEngineModelBinder(IContentsAdminListFilterParser parser) : base(parser)
        {
        }
    }
}

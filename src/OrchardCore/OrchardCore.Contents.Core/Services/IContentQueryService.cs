using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public interface IContentQueryService
    {
        IQuery<ContentItem, ContentItemIndex> GetQueryByOptions(OrchardCore.Contents.Core.Options.ContentOptions options);
    }
}

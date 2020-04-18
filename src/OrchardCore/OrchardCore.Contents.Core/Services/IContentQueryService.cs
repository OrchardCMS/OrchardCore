using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public interface IContentQueryService
    {
        IQuery<ContentItem, ContentItemIndex> GetQueryByOptions(OrchardCore.Contents.ViewModels.ContentOptions options);
    }
}

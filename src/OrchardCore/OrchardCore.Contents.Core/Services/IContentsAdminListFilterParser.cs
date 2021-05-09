using OrchardCore.ContentManagement;
using YesSql.Filters.Query;

namespace OrchardCore.Contents.Services
{
    public interface IContentsAdminListFilterParser : IQueryParser<ContentItem>
    {
    }
}

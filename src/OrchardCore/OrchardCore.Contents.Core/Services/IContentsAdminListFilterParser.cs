using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Filters.Query;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public interface IContentsAdminListFilterParser : IQueryParser<ContentItem>
    {
        
    }
}

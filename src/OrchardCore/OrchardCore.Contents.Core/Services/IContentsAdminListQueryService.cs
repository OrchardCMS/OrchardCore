using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public interface IContentsAdminListQueryService
    {
        Task<IQuery<ContentItem>> QueryAsync(ContentOptionsViewModel model, IUpdateModel updater);
    }
}

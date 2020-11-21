using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public interface IContentsAdminListFilter
    {
        Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater);
    }
}

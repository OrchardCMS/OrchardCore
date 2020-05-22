using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public interface IContentsAdminListFilter
    {
        Task FilterAsync(IQuery<ContentItem> query, IUpdateModel updateModel);

    }
}

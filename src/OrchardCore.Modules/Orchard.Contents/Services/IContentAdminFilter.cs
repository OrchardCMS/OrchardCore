using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public interface IContentAdminFilter
    {
        Task FilterAsync(IQuery<ContentItem> query, ListContentsViewModel model, PagerParameters pagerParameters, IUpdateModel updateModel);
    }
}

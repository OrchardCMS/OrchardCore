using Orchard.ContentManagement;
using Orchard.Contents.ViewModels;
using Orchard.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.Navigation;
using System.Threading.Tasks;
using YesSql.Core.Query;

namespace Orchard.Contents.Services
{
    public interface IContentAdminFilter : ITransientDependency
    {
        Task FilterAsync(IQuery<ContentItem> query, ListContentsViewModel model, PagerParameters pagerParameters, IUpdateModel updateModel);
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public interface IContentAdminFilter
    {
        //TODO I don't see why this needs the model and pager parameters
        Task FilterAsync(IQuery<ContentItem> query, ListContentsViewModel model, PagerParameters pagerParameters, IUpdateModel updateModel);
        Task ApplyRouteValues(ListContentsViewModel model, IUpdateModel updateModel, RouteValueDictionary routeValueDictionary);
    }
}

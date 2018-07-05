using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Navigation;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public interface IQueryFromFilterBox
    {
        Task<IQuery<ContentItem, ContentItemIndex>> ApplyFilterBoxOptionsToQuery(
                        IQuery<ContentItem, ContentItemIndex> query, 
                        FilterBoxViewModel filterBoxViewModel);
    }
}

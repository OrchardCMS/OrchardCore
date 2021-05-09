using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListFilter : IContentsAdminListFilter
    {
        public Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
        {
            if (model.OrderBy == ContentsOrder.Modified)
            {
                query.With<ContentItemIndex>().OrderByDescending(cr => cr.ModifiedUtc);
            }

            if (model.ContentsStatus == ContentsStatus.Draft)
            {
                query.With<ContentItemIndex>(x => x.Latest && !x.Published);
            }

            return Task.CompletedTask;
        }
    }
}

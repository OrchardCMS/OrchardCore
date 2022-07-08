using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.Contents.Services
{
    /// <summary>
    /// Provides custom filters to the content items admin listing.
    /// <see cref="IContentsAdminListFilterProvider"/> is the preferred way to extend the contents list.
    /// This abstraction remains to support backwards compatability, and alternate extension points.
    /// </summary>
    public interface IContentsAdminListFilter
    {
        Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater);
    }
}

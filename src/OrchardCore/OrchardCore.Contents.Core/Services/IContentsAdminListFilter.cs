using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.Contents.Services;

/// <summary>
/// Provides custom filters to the content items admin listing.
/// <see cref="IContentsAdminListFilterProvider"/> is the preferred way to extend the contents list.
/// This abstraction remains to support backwards compatibility, and alternate extension points.
/// </summary>
public interface IContentsAdminListFilter
{
    Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater);

    /// <summary>
    /// Gets whether the query can return a content item several times once <see cref="FilterAsync"/> has run for the current request,
    /// which happens when the filter joins an index with several records per document.
    /// </summary>
    /// <remarks>
    /// The contents list removes the duplicates by grouping the matches by document, so a page reads every match instead of stopping
    /// at the end of the page. The default is <see langword="true"/>, which is always correct. Return <see langword="false"/> only
    /// when the filter adds nothing to the query, or joins indexes with a single record per document.
    /// </remarks>
    bool CanReturnDuplicates => true;
}

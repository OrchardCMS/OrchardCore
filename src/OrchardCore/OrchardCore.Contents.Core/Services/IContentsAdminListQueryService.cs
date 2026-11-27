using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.Contents.Services;

/// <summary>
/// Builds the content admin list's filtered, ordered queries and grouping metadata.
/// </summary>
public interface IContentsAdminListQueryService
{
    /// <summary>
    /// Builds the authorized, filtered and ordered document query.
    /// </summary>
    /// <param name="model">The content list options.</param>
    /// <param name="updater">The update model.</param>
    Task<IQuery<ContentItem>> QueryAsync(ContentOptionsViewModel model, IUpdateModel updater);

    /// <summary>
    /// Returns filtered and ordered index metadata for grouping before row pagination.
    /// Implementations must apply the same authorization and filters as <see cref="QueryAsync"/>.
    /// </summary>
    /// <param name="model">The content list options.</param>
    /// <param name="updater">The update model.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    Task<IReadOnlyList<ContentItemIndex>> QueryIndexAsync(ContentOptionsViewModel model, IUpdateModel updater, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("This content query service must implement index projection to support admin list grouping.");
}

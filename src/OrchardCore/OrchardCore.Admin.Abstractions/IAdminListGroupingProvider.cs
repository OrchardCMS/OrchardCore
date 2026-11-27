using System.Collections;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Admin;

/// <summary>
/// Supplies one grouping option for a named admin list. Register it with AddAdminListGroupingProvider.
/// Query-backed pages invoke the provider on filtered metadata before selecting their page.
/// </summary>
public interface IAdminListGroupingProvider
{
    /// <summary>
    /// The stable option identifier used in URLs and preferences. "None" is reserved.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The localized label in the Group by menu.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Groups the source without filtering out any items. Groups and their items are in display order.
    /// </summary>
    /// <param name="source">The list's source, or its query-backed metadata.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    Task<IReadOnlyList<AdminListGroup>> GroupAsync(IEnumerable source, CancellationToken cancellationToken = default);
}

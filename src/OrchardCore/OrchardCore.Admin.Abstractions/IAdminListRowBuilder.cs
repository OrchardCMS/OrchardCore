using System.Collections;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin;

/// <summary>
/// Builds admin list row shapes from model items after the list resolves its display type.
/// </summary>
public interface IAdminListRowBuilder
{
    /// <summary>
    /// Builds each item once using its model's display manager.
    /// </summary>
    /// <param name="source">The model items, not row shapes.</param>
    /// <param name="itemType">The display model type, or null to infer it from an unambiguous IEnumerable&lt;T&gt;.</param>
    /// <param name="displayType">The row display type selected by the list.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IList<IShape>> BuildAsync(IEnumerable source, Type itemType, string displayType, CancellationToken cancellationToken = default);
}

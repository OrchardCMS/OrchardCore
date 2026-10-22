using OrchardCore.Admin;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Indexing.ViewModels;

/// <summary>
/// The queried index profiles and options for the indexing admin list.
/// </summary>
public sealed class AdminIndexViewModel : ListSourcedEntitiesViewModel<IndexProfileKey, IndexProfile, IndexingEntityOptions>
{
    public IEnumerable<IndexSourceGroupViewModel> SourceGroups { get; set; } = [];

    /// <summary>
    /// The data context for rendering the index profiles with the configured layout.
    /// </summary>
    public AdminListContext List { get; set; }
}

public sealed class IndexSourceGroupViewModel
{
    public string ProviderName { get; set; } = string.Empty;

    public string ProviderDisplayName { get; set; } = string.Empty;

    public IEnumerable<IndexProfileKey> Sources { get; set; } = [];
}

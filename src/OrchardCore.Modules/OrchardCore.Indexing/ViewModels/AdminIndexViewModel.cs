using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Indexing.ViewModels;

public sealed class AdminIndexViewModel : ListSourcedEntitiesViewModel<IndexProfileKey, ModelEntry<IndexProfile>, IndexingEntityOptions>
{
    public IEnumerable<IndexSourceGroupViewModel> SourceGroups { get; set; } = [];

    /// <summary>
    /// The <c>AdminList</c> shape rendering the index profiles with the configured layout.
    /// </summary>
    public dynamic List { get; set; }
}

public sealed class IndexSourceGroupViewModel
{
    public string ProviderName { get; set; } = string.Empty;

    public string ProviderDisplayName { get; set; } = string.Empty;

    public IEnumerable<IndexProfileKey> Sources { get; set; } = [];
}

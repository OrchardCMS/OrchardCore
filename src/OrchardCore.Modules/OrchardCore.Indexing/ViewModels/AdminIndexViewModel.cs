using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Indexing.ViewModels;

public sealed class AdminIndexViewModel : ListSourcedEntitiesViewModel<IndexProfileKey, ModelEntry<IndexProfile>, IndexingEntityOptions>
{
    public IEnumerable<IndexSourceGroupViewModel> SourceGroups { get; set; } = [];
}

public sealed class IndexSourceGroupViewModel
{
    public string ProviderName { get; set; } = string.Empty;

    public string ProviderDisplayName { get; set; } = string.Empty;

    public IEnumerable<IndexProfileKey> Sources { get; set; } = [];
}

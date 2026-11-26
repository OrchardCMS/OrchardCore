using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Secrets.ViewModels;

public class SecretStoreViewModel
{
    public string SourceStore { get; set; }
    public string DestinationStore { get; set; }
    public string Name { get; set; }
    public bool Confirm { get; set; }
    [BindNever]
    public int? ActiveCount { get; set; }
    [BindNever]
    public IList<string> AvailableStores { get; set; } = [];
    [BindNever]
    public IReadOnlyList<SecretStoreOperationResult> Results { get; set; } = [];

    /// <summary>
    /// Gets or sets the editor of the move of secrets to another store, built by the display drivers of <see cref="SecretStoreViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}

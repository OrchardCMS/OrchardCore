using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Secrets.ViewModels;

public sealed class SecretStoreViewModel
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
}

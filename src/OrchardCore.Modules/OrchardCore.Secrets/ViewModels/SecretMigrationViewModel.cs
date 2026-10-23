using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Secrets.ViewModels;

public class SecretMigrationViewModel
{
    public string Store { get; set; }

    [BindNever]
    public IList<string> AvailableStores { get; set; } = [];

    [BindNever]
    public dynamic Editor { get; set; }

    [BindNever]
    public IList<SecretMigrationResult> Results { get; set; } = [];
}

using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.ViewModels;

public class SecretBindingEntry
{
    public string Name { get; set; }
    public SecretBinding SecretBinding { get; set; }
    public dynamic Summary { get; set; }
}

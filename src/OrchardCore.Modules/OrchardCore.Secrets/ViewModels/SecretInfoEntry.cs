using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.ViewModels;

public class SecretInfoEntry
{
    public string Name { get; set; }
    public SecretInfo Info { get; set; }
    public dynamic Summary { get; set; }
}

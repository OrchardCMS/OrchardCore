namespace OrchardCore.Secrets.Models;

public class SecretStoreDescriptor
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool IsReadOnly { get; set; }
}

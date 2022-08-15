namespace OrchardCore.ResourceManagement;

public class ResourceSettings
{
    public OptionSource Source { get; set; }

    public ResourceOptions Options { get; set; } = new();
}

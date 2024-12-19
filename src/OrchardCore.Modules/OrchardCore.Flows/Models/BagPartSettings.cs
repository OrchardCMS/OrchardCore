namespace OrchardCore.Flows.Models;

public class BagPartSettings
{
    public string[] ContainedContentTypes { get; set; } = [];

    public string[] ContainedStereotypes { get; set; } = [];

    public string DisplayType { get; set; }
}

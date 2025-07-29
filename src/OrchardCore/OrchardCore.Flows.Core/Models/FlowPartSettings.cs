namespace OrchardCore.Flows.Models;

public class FlowPartSettings
{
    public string[] ContainedContentTypes { get; set; } = [];

    public bool CollapseContainedItems { get; set; }
}

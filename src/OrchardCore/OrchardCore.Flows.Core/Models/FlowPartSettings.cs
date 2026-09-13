using System.ComponentModel;

namespace OrchardCore.Flows.Models;

public class FlowPartSettings
{
    [Description("The widget content types that editors can add directly to the Flow.")]
    public string[] ContainedContentTypes { get; set; } = [];

    [Description("Whether contained widget editors are collapsed initially.")]
    public bool CollapseContainedItems { get; set; }

    [Description("The default alignment assigned to widgets added to the Flow.")]
    public FlowAlignment? DefaultAlignment { get; set; }
}

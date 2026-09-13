using System.ComponentModel;

namespace OrchardCore.Flows.Models;

public class BagPartSettings
{
    [Description("The content types allowed in the Bag.")]
    public string[] ContainedContentTypes { get; set; } = [];

    [Description("The content type stereotypes allowed in the Bag.")]
    public string[] ContainedStereotypes { get; set; } = [];

    [Description("The display type used to render contained items.")]
    public string DisplayType { get; set; }

    [Description("Whether contained item editors are collapsed initially.")]
    public bool CollapseContainedItems { get; set; }
}

using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Media.Fields;

public class MediaField : ContentField
{
    [Description("The paths of the selected media assets.")]
    public string[] Paths { get; set; }

    [Description("The alternative text values for the selected media assets, in the same order as Paths.")]
    public string[] MediaTexts { get; set; }
}

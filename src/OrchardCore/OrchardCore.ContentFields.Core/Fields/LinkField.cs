using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class LinkField : ContentField
{
    [Description("The link URL.")]
    public string Url { get; set; }

    [Description("The text displayed for the link.")]
    public string Text { get; set; }

    [Description("The browsing context in which to open the link, such as _blank.")]
    public string Target { get; set; }
}

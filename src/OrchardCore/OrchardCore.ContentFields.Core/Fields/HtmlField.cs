using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields;

public class HtmlField : ContentField
{
    [Description("The HTML content.")]
    public string Html { get; set; }
}

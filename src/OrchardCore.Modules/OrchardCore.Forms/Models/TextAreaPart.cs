using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class TextAreaPart : ContentPart
{
    [Description("The initial text in the text area.")]
    public string DefaultValue { get; set; }

    [Description("The hint displayed when the text area is empty.")]
    public string Placeholder { get; set; }

    [Description("The visible height of the text area in text rows.")]
    public int? Rows { get; set; }
}

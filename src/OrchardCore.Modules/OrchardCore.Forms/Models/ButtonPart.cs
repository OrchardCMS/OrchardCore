using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class ButtonPart : ContentPart
{
    [Description("The text displayed on the button.")]
    public string Text { get; set; }

    [Description("The HTML button type, such as submit, button, or reset.")]
    public string Type { get; set; }
}

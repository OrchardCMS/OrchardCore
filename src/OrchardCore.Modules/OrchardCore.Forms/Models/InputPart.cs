using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class InputPart : ContentPart
{
    [Description("The HTML input type, such as text, number, email, or checkbox.")]
    public string Type { get; set; }

    [Description("The initial value of the input.")]
    public string DefaultValue { get; set; }

    [Description("The hint displayed when the input is empty.")]
    public string Placeholder { get; set; }
}

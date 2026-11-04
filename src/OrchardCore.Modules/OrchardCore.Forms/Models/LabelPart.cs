using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class LabelPart : ContentPart
{
    [Description("The HTML id of the form element associated with the label.")]
    public string For { get; set; }
}

using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class ValidationPart : ContentPart
{
    [Description("The HTML id of the form element whose validation message is displayed.")]
    public string For { get; set; }
}

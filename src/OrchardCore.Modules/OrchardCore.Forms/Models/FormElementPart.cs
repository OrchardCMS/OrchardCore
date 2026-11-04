using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

/// <summary>
/// Turns a content item into a form element.
/// </summary>
public class FormElementPart : ContentPart
{
    [Description("The HTML id attribute of the form element.")]
    public string Id { get; set; }
}

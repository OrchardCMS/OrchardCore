using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

/// <summary>
/// Turns a content item into a form element that supports input.
/// </summary>
public class FormInputElementPart : ContentPart
{
    [Description("The name used for the input value in submitted form data.")]
    public string Name { get; set; }
}

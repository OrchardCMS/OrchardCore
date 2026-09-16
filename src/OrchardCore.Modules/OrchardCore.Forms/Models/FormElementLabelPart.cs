using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class FormElementLabelPart : ContentPart
{
    [Description("How the form element's label is rendered.")]
    public LabelOptions Option { get; set; }

    [Description("The text displayed in the form element's label.")]
    public string Label { get; set; }
}

public class FormElementValidationPart : ContentPart
{
    [Description("How validation messages are rendered for the form element.")]
    public ValidationOptions Option { get; set; }
}

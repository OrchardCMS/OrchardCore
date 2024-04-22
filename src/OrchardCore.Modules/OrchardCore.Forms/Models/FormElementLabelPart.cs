using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class FormElementLabelPart : ContentPart
{
    public LabelOptions Option { get; set; }

    public string Label { get; set; }
}

public class FormElementValidationPart : ContentPart
{
    public ValidationOptions Option { get; set; }
}

using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class FormInputElementVisibilityPart : ContentPart
{
    public IEnumerable<FormVisibilityRuleGroup> Groups { get; set; }

    public FormVisibilityAction Action { get; set; } = FormVisibilityAction.None;
}

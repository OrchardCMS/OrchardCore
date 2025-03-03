using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public class FormInputElementVisibilityPart : ContentPart
{
    public IEnumerable<FormVisibilityRuleGroup> Groups { get; set; }

    public string TargetInputId { get; set; }  // The ID of the input widget this rule applies to

    public FormVisibilityAction Action { get; set; } = FormVisibilityAction.None;  // Default to None
}

using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models;

public sealed class FormInputElementVisibilityPart : ContentPart
{
    [Description("The groups of conditions that control the form element's visibility.")]
    public IEnumerable<FormVisibilityRuleGroup> Groups { get; set; }

    [Description("The visibility action applied when the conditions are met.")]
    public FormVisibilityAction Action { get; set; } = FormVisibilityAction.None;
}

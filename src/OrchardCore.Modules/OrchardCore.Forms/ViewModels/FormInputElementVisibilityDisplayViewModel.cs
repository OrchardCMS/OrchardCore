using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels;

public class FormInputElementVisibilityDisplayViewModel
{
    public string ElementName { get; set; }

    public FormVisibilityAction Action { get; set; }

    public IEnumerable<FormVisibilityRuleGroupDisplayViewModel> Groups { get; set; }
}

using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels;

public class FormInputElementVisibilityDisplayViewModel
{
    public string ElementName { get; set; }

    public FormVisibilityAction Action { get; set; } = FormVisibilityAction.None;

    public IEnumerable<FormVisibilityRuleGroupDisplayViewModel> Groups { get; set; }
}

public class FormVisibilityRuleGroupDisplayViewModel
{
    public IEnumerable<FormVisibilityRuleDisplayViewModel> Rules { get; set; }
}

public class FormVisibilityRuleDisplayViewModel
{
    public string Field { get; set; }
    public string Operator { get; set; }
    public string Value { get; set; }
}

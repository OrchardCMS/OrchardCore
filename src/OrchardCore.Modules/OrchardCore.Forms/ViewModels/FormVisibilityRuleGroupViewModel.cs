namespace OrchardCore.Forms.ViewModels;

public class FormVisibilityRuleGroupViewModel
{
    public bool IsRemoved { get; set; }

    public IList<FormVisibilityRuleViewModel> Rules { get; set; }
}

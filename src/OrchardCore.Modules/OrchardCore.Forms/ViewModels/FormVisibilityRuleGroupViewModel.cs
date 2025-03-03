namespace OrchardCore.Forms.ViewModels;

public class FormVisibilityRuleGroupViewModel
{
    public bool IsRemoved { get; set; } // If true, this group will be ignored

    public IList<FormVisibilityRuleViewModel> Rules { get; set; }
}

// This ViewModel represents a group of rules that act as an OR (||) condition.
// If at least one rule inside this group is true, the group is considered "true."
// Groups help organize rules and allow for complex form logic by combining multiple rule sets.


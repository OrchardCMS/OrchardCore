using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels;

public class FormInputElementVisibilityViewModel
{
    public FormVisibilityAction Action { get; set; } = FormVisibilityAction.Hide;  // Show or Hide

    public List<FormVisibilityRuleGroupViewModel> Groups { get; set; } // Groups of rules

    [BindNever]
    public IEnumerable<SelectListItem> Actions { get; set; } // Dropdown options
}

// This ViewModel controls the visibility of an input field (checkbox) based on rules and conditions.
// It connects to a specific input field using and determines whether to show or hide it.
// The visibility logic is based on Groups (OR conditions) and Rules (AND conditions).


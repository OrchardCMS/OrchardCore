using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels;

public class FormInputElementVisibilityViewModel
{
    public FormVisibilityAction Action { get; set; }  // Show or Hide

    public IEnumerable<SelectListItem> Actions { get; set; } // Dropdown options

    public IList<FormVisibilityRuleGroupViewModel> Groups { get; set; } // Groups of rules

    public string TargetInputId { get; set; }  // The ID of the input widget this rule applies to
}

// This ViewModel controls the visibility of an input field (checkbox) based on rules and conditions.
// It connects to a specific input field using TargetInputId and determines whether to show or hide it.
// The visibility logic is based on Groups (OR conditions) and Rules (AND conditions).


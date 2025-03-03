using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels;

public class FormVisibilityRuleViewModel
{
    public string Field { get; set; }

    public FormVisibilityOperator Operator { get; set; }

    public bool IsRemoved { get; set; }

    public string Value { get; set; }

    public string TargetInputId { get; set; }  // 🔹 The ID of the input this rule applies to

    [BindNever]
    public IEnumerable<FormVisibilityFieldViewModel> Fields { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Operators { get; set; }
}

// This ViewModel represents a single rule inside a group, acting as an AND (&&) condition.
// A rule checks one specific field and applies an operator (e.g., Is, Contains, GreaterThan).
// If all rules inside a group are true, the group is considered "true."


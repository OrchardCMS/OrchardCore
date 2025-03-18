using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels;

public class FormVisibilityRuleViewModel
{
    public string Field { get; set; } = "";

    public FormVisibilityOperator? Operator { get; set; }

    public bool IsRemoved { get; set; }

    public string Value { get; set; }

    public string ConditionalField { get; set; }  //Field that should be shown/hidden

    public bool ShowWhenMatched { get; set; }  //Should we show or hide the field when condition is met?

    [BindNever]
    public IEnumerable<FormVisibilityFieldViewModel> Fields { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Operators { get; set; }
}

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels;

public class FormInputElementVisibilityViewModel
{
    public FormVisibilityAction Action { get; set; } = FormVisibilityAction.None;

    public List<FormVisibilityRuleGroupViewModel> Groups { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Actions { get; set; }
}

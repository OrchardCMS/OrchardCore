using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels;

public class FormInputElementVisibilityViewModel
{
    public FormVisibilityAction Action { get; set; }

    public IEnumerable<SelectListItem> Actions { get; set; }

    public IList<FormVisibilityRuleGroupViewModel> Groups { get; set; }
}

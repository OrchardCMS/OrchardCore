using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Forms.ViewModels;

public class FormVisibilityRuleViewModel
{
    public string Field { get; set; }

    public string Operator { get; set; }

    public string Value { get; set; }

    public bool CaseSensitive { get; set; }

    [BindNever]
    public IEnumerable<FormVisibilityFieldViewModel> Fields { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Operators { get; set; }
}

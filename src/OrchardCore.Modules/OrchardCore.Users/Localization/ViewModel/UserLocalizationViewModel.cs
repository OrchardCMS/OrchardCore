using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Users.Localization.ViewModels;

public class UserLocalizationViewModel
{
    public string Culture { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> SupportedCultures { get; set; } = new List<SelectListItem>();
}

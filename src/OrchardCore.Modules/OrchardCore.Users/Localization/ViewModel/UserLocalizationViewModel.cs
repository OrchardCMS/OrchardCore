using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Users.Localization.ViewModels;

public class UserLocalizationViewModel
{
    public string SelectedCulture { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> CultureList { get; set; } = new List<SelectListItem>();
}

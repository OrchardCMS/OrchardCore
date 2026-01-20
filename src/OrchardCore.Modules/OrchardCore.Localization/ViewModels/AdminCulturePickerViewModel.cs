using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Localization.ViewModels;

public class AdminCulturePickerViewModel
{
    [BindNever]
    public CultureInfo CurrentCulture { get; set; }

    [BindNever]
    public IEnumerable<CultureInfo> SupportedCultures { get; set; }
}

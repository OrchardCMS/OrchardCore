using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.ContentLocalization.ViewModels;

public class CulturePickerViewModel
{
    [BindNever]
    public CultureInfo CurrentCulture { get; set; }

    [BindNever]
    public IEnumerable<CultureInfo> SupportedCultures { get; set; }
}

using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentLocalization.ViewModels;

public class LocalizationPartViewModel : ShapeViewModel
{
    public string LocalizationSet { get; set; }
    public string Culture { get; set; }

    [BindNever]
    public CultureInfo CultureInfo => CultureInfo.GetCultureInfo(Culture);

    [BindNever]
    public LocalizationPart LocalizationPart { get; set; }

    [BindNever]
    public IEnumerable<LocalizationLinksViewModel> ContentItemCultures { get; set; }
}

public class LocalizationLinksViewModel
{
    public bool IsDeleted { get; set; }
    public string ContentItemId { get; set; }
    public CultureInfo Culture { get; set; }
}

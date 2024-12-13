using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.ContentTypes.ViewModels;

public class ListContentPartsViewModel
{
    [BindNever]
    public IEnumerable<EditPartViewModel> Parts { get; set; }
}

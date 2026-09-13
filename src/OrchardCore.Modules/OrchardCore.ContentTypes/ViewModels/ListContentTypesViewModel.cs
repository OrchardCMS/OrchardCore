using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.ContentTypes.ViewModels;

public class ListContentTypesViewModel
{
    public bool ShowGrouping { get; set; } = true;

    [BindNever]
    public IEnumerable<EditTypeViewModel> Types { get; set; }
}

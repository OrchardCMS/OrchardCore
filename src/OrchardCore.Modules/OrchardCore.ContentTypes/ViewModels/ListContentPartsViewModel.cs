using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Admin;

namespace OrchardCore.ContentTypes.ViewModels;

public class ListContentPartsViewModel
{
    [BindNever]
    public IEnumerable<EditPartViewModel> Parts { get; set; }

    /// <summary>
    /// The data context for rendering the rows in the configured layout.
    /// </summary>
    public AdminListContext List { get; set; }
}

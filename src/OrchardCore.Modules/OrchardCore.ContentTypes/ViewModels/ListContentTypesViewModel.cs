using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.ContentTypes.ViewModels;

public class ListContentTypesViewModel
{
    [BindNever]
    public IEnumerable<EditTypeViewModel> Types { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the rows in the configured layout.
    /// </summary>
    public dynamic List { get; set; }
}

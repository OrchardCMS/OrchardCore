using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentTypes.Models;

namespace OrchardCore.ContentTypes.ViewModels;

public class ListContentTypesViewModel
{
    [BindNever]
    public IEnumerable<EditType> Types { get; set; }
}

using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentTypes.Models;

namespace OrchardCore.ContentTypes.ViewModels;

public class ListContentPartsViewModel
{
    [BindNever]
    public IEnumerable<EditPart> Parts { get; set; }
}

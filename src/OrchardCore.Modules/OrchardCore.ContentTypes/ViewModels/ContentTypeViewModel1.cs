using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentTypes.ViewModels;

public class CreateContentTypeViewModel
{
    public string Suggestion { get; set; }

    [BindNever]
    public IShape Editor { get; internal set; }
}

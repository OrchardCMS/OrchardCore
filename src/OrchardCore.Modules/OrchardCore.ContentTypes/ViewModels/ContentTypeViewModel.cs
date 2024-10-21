using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.ContentTypes.ViewModels;

public class CreateContentViewModel
{
    public string DisplayName { get; set; }

    public string Name { get; set; }

    [BindNever]
    public bool IsNew { get; internal set; }
}

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Indexing.ViewModels;

public class EditIndexEntityViewModel
{
    public string IndexName { get; set; }

    public string DisplayText { get; set; }

    [BindNever]
    public bool IsNew { get; set; }
}

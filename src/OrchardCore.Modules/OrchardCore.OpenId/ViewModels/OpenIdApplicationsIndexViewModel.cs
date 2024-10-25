using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.OpenId.ViewModels;

public class OpenIdApplicationsIndexViewModel
{
    [BindNever]
    public IList<OpenIdApplicationEntry> Applications { get; set; }

    [BindNever]
    public dynamic Pager { get; set; }
}

public class OpenIdApplicationEntry
{
    public string DisplayName { get; set; }
    public string Id { get; set; }
    public bool IsChecked { get; set; }
}

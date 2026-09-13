using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.OpenId.ViewModels;

public class OpenIdApplicationsIndexViewModel
{
    public string SearchText { get; set; }

    [BindNever]
    public IList<OpenIdApplicationEntry> Applications { get; set; }

    [BindNever]
    public dynamic Pager { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the applications and the pager in the configured layout.
    /// </summary>
    [BindNever]
    public dynamic List { get; set; }
}

public class OpenIdApplicationEntry
{
    public string DisplayName { get; set; }
    public string Id { get; set; }
    public bool IsChecked { get; set; }
}

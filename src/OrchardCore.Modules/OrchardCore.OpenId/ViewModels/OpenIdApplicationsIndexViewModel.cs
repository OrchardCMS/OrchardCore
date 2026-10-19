using OrchardCore.Admin;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.OpenId.ViewModels;

public class OpenIdApplicationsIndexViewModel
{
    public string SearchText { get; set; }

    [BindNever]
    public IList<OpenIdApplicationEntry> Applications { get; set; }

    [BindNever]
    public IShape Pager { get; set; }

    /// <summary>
    /// The data context for rendering the applications and the pager in the configured layout.
    /// </summary>
    [BindNever]
    public AdminListContext List { get; set; }
}

public class OpenIdApplicationEntry
{
    public string DisplayName { get; set; }
    public string Id { get; set; }
    public bool IsChecked { get; set; }
}

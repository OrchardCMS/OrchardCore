using OrchardCore.Admin;
using OrchardCore.DisplayManagement;

namespace OrchardCore.OpenId.ViewModels;

public class OpenIdScopeIndexViewModel
{
    public string SearchText { get; set; }

    public IList<OpenIdScopeEntry> Scopes { get; } = [];
    public IShape Pager { get; set; }

    /// <summary>
    /// The data context for rendering the scopes and the pager in the configured layout.
    /// </summary>
    public AdminListContext List { get; set; }
}

public class OpenIdScopeEntry
{
    public string Description { get; set; }
    public string DisplayName { get; set; }
    public string Id { get; set; }
    public bool IsChecked { get; set; }
    public string Name { get; set; }
}

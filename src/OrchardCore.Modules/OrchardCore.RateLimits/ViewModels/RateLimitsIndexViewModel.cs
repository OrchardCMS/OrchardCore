using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.ViewModels;

public class RateLimitsIndexViewModel
{
    public string SearchText { get; set; }

    public RateLimitPolicyBulkAction BulkAction { get; set; }

    /// <summary>
    /// The queried policies on this page, before their row shapes are built.
    /// </summary>
    public IList<RateLimitPolicy> Policies { get; set; } = [];

    public IList<BuiltInRouteRateLimitViewModel> BuiltInRouteLimits { get; set; } = [];

    public IList<BuiltInGroupRateLimitViewModel> BuiltInGroupLimits { get; set; } = [];

    public IList<SelectListItem> BulkActions { get; set; } = [];

    [BindNever]
    public IShape Pager { get; set; }

    /// <summary>
    /// The data context for rendering the policies, the toolbar and the pager in the configured layout.
    /// </summary>
    [BindNever]
    public AdminListContext List { get; set; }
}

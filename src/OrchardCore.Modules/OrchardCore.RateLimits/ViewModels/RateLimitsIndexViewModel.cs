using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.RateLimits.ViewModels;

public class RateLimitsIndexViewModel
{
    public string SearchText { get; set; }

    public RateLimitPolicyBulkAction BulkAction { get; set; }

    public IList<RateLimitPolicyEntryViewModel> Policies { get; set; } = [];

    public IList<BuiltInRouteRateLimitViewModel> BuiltInRouteLimits { get; set; } = [];

    public IList<BuiltInGroupRateLimitViewModel> BuiltInGroupLimits { get; set; } = [];

    public IList<SelectListItem> BulkActions { get; set; } = [];

    [BindNever]
    public dynamic Pager { get; set; }

    /// <summary>
    /// The <c>AdminList</c> shape rendering the policies, the toolbar and the pager in the configured layout.
    /// </summary>
    [BindNever]
    public dynamic List { get; set; }
}

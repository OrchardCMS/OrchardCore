using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.RateLimits.ViewModels;

public class RateLimitsIndexViewModel
{
    public string SearchText { get; set; }

    public IList<RateLimitPolicyEntryViewModel> Policies { get; set; } = [];

    public IList<BuiltInRouteRateLimitViewModel> BuiltInRouteLimits { get; set; } = [];

    public IList<SelectListItem> BulkActions { get; set; } = [];
}

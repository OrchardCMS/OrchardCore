using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.ViewModels;

public class RateLimitPolicyEditViewModel
{
    public string PolicyId { get; set; }

    public RateLimitPolicy Policy { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public RateLimitPolicyScope Scope { get; set; }

    public string RouteName { get; set; }

    public string Path { get; set; }

    public RateLimitPolicyStatus Status { get; set; }

    public DateTime? PublishedUtc { get; set; }

    public string PublishedTargetDescription { get; set; }

    [BindNever]
    public IList<ModelEntry<RateLimitLimiter>> DraftLimiters { get; set; } = [];

    [BindNever]
    public IList<ModelEntry<RateLimitLimiter>> PublishedLimiters { get; set; } = [];

    [BindNever]
    public IList<RateLimiterSourceViewModel> LimiterSources { get; set; } = [];

    [BindNever]
    public IEnumerable<SelectListItem> RouteNames { get; set; } = [];

    [BindNever]
    public IEnumerable<SelectListItem> PolicyScopes { get; set; } = [];
}

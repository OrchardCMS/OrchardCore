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

    public string Path { get; set; }

    public string GroupName { get; set; }

    public bool IsEnabled { get; set; }

    public bool InitialIsEnabled { get; set; }

    public RateLimitPolicyStatus Status { get; set; }

    public DateTime? EnabledUtc { get; set; }

    [BindNever]
    public IList<ModelEntry<RateLimitLimiter>> Limiters { get; set; } = [];

    [BindNever]
    public IList<RateLimiterSourceViewModel> LimiterSources { get; set; } = [];

    [BindNever]
    public IEnumerable<SelectListItem> PolicyScopes { get; set; } = [];

    [BindNever]
    public IEnumerable<SelectListItem> AvailableGroups { get; set; } = [];

    [BindNever]
    public bool AreLimiterChangesAllowed { get; set; }

    [BindNever]
    public string PublishedPolicyLimiterMessage { get; set; }
}

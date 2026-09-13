using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.ViewModels;

public class RateLimitPolicyEntryViewModel
{
    public string PolicyId { get; set; }

    public dynamic ActionsMenu { get; set; }

    /// <summary>
    /// The full <c>SummaryAdmin</c> row shape rendered by the <c>AdminList</c> shape.
    /// </summary>
    public dynamic Shape { get; set; }

    public RateLimitPolicy Policy { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public RateLimitPolicyStatus Status { get; set; }

    public DateTime? EnabledUtc { get; set; }

    public bool IsEnabled { get; set; }
}

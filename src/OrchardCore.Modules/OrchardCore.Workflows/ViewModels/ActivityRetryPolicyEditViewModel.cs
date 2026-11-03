using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

public class ActivityRetryPolicyEditViewModel
{
    public int MaxRetries { get; set; }

    public int DelaySeconds { get; set; }

    public ActivityRetryBackoff Backoff { get; set; }

    public ActivityFailureBehavior OnFailure { get; set; }
}

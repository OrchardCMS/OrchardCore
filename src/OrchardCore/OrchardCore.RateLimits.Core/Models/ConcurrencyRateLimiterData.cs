using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits.Models;

public sealed class ConcurrencyRateLimiterData
{
    public int PermitLimit { get; set; }

    public int QueueLimit { get; set; }

    public QueueProcessingOrder QueueProcessingOrder { get; set; }
}

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits.ViewModels;

public class TokenBucketRateLimiterViewModel
{
    public int TokenLimit { get; set; }

    public int QueueLimit { get; set; }

    public int TokensPerPeriod { get; set; }

    public int ReplenishmentPeriodSeconds { get; set; }

    public QueueProcessingOrder QueueProcessingOrder { get; set; }

    public IEnumerable<SelectListItem> QueueProcessingOrders { get; set; } = [];
}

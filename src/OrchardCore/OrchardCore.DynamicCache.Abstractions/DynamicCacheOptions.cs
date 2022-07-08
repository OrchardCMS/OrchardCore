using System;

namespace OrchardCore.DynamicCache
{
    public class DynamicCacheOptions
    {
        public TimeSpan? FailoverRetryLatency { get; set; }
    }
}

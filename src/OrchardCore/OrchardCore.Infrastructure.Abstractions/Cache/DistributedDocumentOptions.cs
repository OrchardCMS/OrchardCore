using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Infrastructure.Cache
{
    public class DistributedDocumentOptions : DistributedCacheEntryOptions
    {
        public bool? CheckConcurrency { get; set; }
        public bool? CheckConsistency { get; set; }
    }
}

using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Documents.Options
{
    public class DocumentOptions : DistributedCacheEntryOptions
    {
        public string CacheKey { get; set; }
        public string CacheIdKey { get; set; }
        public bool? CheckConcurrency { get; set; }
        public bool? CheckConsistency { get; set; }
    }
}

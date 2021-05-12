using System;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Documents.Options
{
    public class DocumentOptions : DistributedCacheEntryOptions
    {
        public string CacheKey { get; set; }
        public string CacheIdKey { get; set; }
        public bool? CheckConcurrency { get; set; }
        public bool? CheckConsistency { get; set; }
        public TimeSpan? SynchronizationLatency { get; set; }
        public IDocumentSerialiser Serializer { get; set; }
        public int CompressThreshold { get; set; }

        // Only used by an explicit atomic update.
        public int LockTimeout { get; set; }
        public int LockExpiration { get; set; }
    }
}

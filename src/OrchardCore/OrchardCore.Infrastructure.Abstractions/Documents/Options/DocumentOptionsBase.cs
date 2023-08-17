using System;
using Microsoft.Extensions.Caching.Distributed;

namespace OrchardCore.Documents.Options
{
    public class DocumentOptionsBase : DistributedCacheEntryOptions
    {
        // From the named or shared config or default.
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

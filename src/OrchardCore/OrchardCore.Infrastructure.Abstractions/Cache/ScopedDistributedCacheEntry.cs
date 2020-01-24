using System;
using Newtonsoft.Json;

namespace OrchardCore.Infrastructure.Cache
{
    public class ScopedDistributedCacheEntry
    {
        public ScopedDistributedCacheEntry() => GenerateCacheId();

        public string CacheId { get; set; }

        [JsonIgnore]
        public bool HasSliding { get; set; }

        public virtual void GenerateCacheId() => CacheId = Guid.NewGuid().ToString();
    }
}

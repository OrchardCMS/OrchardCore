using Newtonsoft.Json;

namespace OrchardCore.Infrastructure.Cache
{
    public class ScopedDistributedCacheData
    {
        public string Identifier { get; set; }

        [JsonIgnore]
        public bool HasSlidingExpiration { get; set; }
    }
}

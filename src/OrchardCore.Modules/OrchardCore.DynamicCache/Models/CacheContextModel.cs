using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.Models
{
    /// <summary>
    /// A serialiazable and deserializable model to represent a <see cref="CacheContext"/>
    /// </summary>
    public class CacheContextModel
    {
        public string CacheId { get; set; }
        public ICollection<string> Contexts { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public DateTimeOffset? ExpiresOn { get; set; }
        public TimeSpan? ExpiresAfter { get; set; }
        public TimeSpan? ExpiresSliding { get; set; }

        internal static CacheContextModel FromCacheContext(CacheContext cacheContext)
        {
            return new CacheContextModel
            {
                CacheId = cacheContext.CacheId,
                Contexts = cacheContext.Contexts,
                Tags = cacheContext.Tags,
                ExpiresOn = cacheContext.ExpiresOn,
                ExpiresAfter = cacheContext.ExpiresAfter,
                ExpiresSliding = cacheContext.ExpiresSliding
            };
        }

        internal CacheContext ToCacheContext()
        {
            var cacheContext = new CacheContext(CacheId)
                .AddContext(Contexts.ToArray())
                .AddTag(Tags.ToArray());

            if (ExpiresOn.HasValue)
            {
                cacheContext.WithExpiryOn(ExpiresOn.Value);
            }

            if (ExpiresAfter.HasValue)
            {
                cacheContext.WithExpiryAfter(ExpiresAfter.Value);
            }

            if (ExpiresSliding.HasValue)
            {
                cacheContext.WithExpirySliding(ExpiresSliding.Value);
            }

            return cacheContext;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.Models
{
    public class EdgeSideInclude
    {
        public string CacheId { get; set; }
        public ICollection<string> Contexts { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<string> Dependencies { get; set; }
        public TimeSpan? Duration { get; set; }
        public TimeSpan? SlidingExpirationWindow { get; set; }

        internal static EdgeSideInclude FromCacheContext(CacheContext cacheContext)
        {
            return new EdgeSideInclude
            {
                CacheId = cacheContext.CacheId,
                Contexts = cacheContext.Contexts,
                Tags = cacheContext.Tags,
                Dependencies = cacheContext.Dependencies,
                Duration = cacheContext.Duration,
                SlidingExpirationWindow = cacheContext.SlidingExpirationWindow
            };
        }

        internal CacheContext ToCacheContext()
        {
            var cacheContext = new CacheContext(CacheId)
                .AddContext(Contexts.ToArray())
                .AddTag(Tags.ToArray())
                .AddDependency(Dependencies.ToArray());

            if (Duration.HasValue)
            {
                cacheContext.WithDuration(Duration.Value);
            }

            if (SlidingExpirationWindow.HasValue)
            {
                cacheContext.WithSlidingExpiration(SlidingExpirationWindow.Value);
            }

            return cacheContext;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Media.Processing;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;

namespace OrchardCore.Benchmark
{
    [MemoryDiagnoser]
    public class MediaTokenServiceBenchmark
    {
        private static readonly MediaTokenService _mediaTokenService;
        private static readonly MediaTokenService _mediaTokenServiceWithoutCache;

        static MediaTokenServiceBenchmark()
        {
            IMemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            IMemoryCache nullCache = new NullCache();

            using var rng = RandomNumberGenerator.Create();
            var hashKey = new byte[64];
            rng.GetBytes(hashKey);

            // mimic default configuration
            var options = Options.Create(new MediaTokenOptions { HashKey = hashKey });
            var processors = new IImageWebProcessor[]
            {
                new ResizeWebProcessor(),
                new FormatWebProcessor(Options.Create(new ImageSharpMiddlewareOptions())),
                new BackgroundColorWebProcessor(),
                new QualityWebProcessor(),
                new ImageVersionProcessor(),
                new TokenCommandProcessor()
            };

            _mediaTokenService = new MediaTokenService(memoryCache, options, processors);
            _mediaTokenServiceWithoutCache = new MediaTokenService(nullCache, options, processors);
        }

        [Benchmark]
#pragma warning disable CA1822 // Mark members as static
        public string AddTokenToPath()
        {
            return _mediaTokenService.AddTokenToPath("/media/portfolio/1.jpg?width=600&height=480&rmode=stretch");
        }

        [Benchmark]
        public string AddTokenToPath_NoCache()
        {
            return _mediaTokenServiceWithoutCache.AddTokenToPath("/media/portfolio/1.jpg?width=600&height=480&rmode=stretch");
        }

        [Benchmark]
        public string AddTokenToPath_LongPath()
        {
            return _mediaTokenService.AddTokenToPath("/media/portfolio/1.jpg?width=LOOOOOOOOOOOOOOONG&height=LOOOOOOOOOOOOOOONG&rmode=LOOOOOOOOOOOOOOONG&rxy=LOOOOOOOOOOOOOOONG&rsampler=LOOOOOOOOOOOOOOONG&ranchor=LOOOOOOOOOOOOOOONG&compand=LOOOOOOOOOOOOOOONG&token=LOOOOOOOOOOOOOOONG&quality=LOOOOOOOOOOOOOOONG");
        }

        [Benchmark]
        public string AddTokenToPath_LongPath_NoCache()
        {
            return _mediaTokenServiceWithoutCache.AddTokenToPath("/media/portfolio/1.jpg?width=LOOOOOOOOOOOOOOONG&height=LOOOOOOOOOOOOOOONG&rmode=LOOOOOOOOOOOOOOONG&rxy=LOOOOOOOOOOOOOOONG&rsampler=LOOOOOOOOOOOOOOONG&ranchor=LOOOOOOOOOOOOOOONG&compand=LOOOOOOOOOOOOOOONG&token=LOOOOOOOOOOOOOOONG&quality=LOOOOOOOOOOOOOOONG");
        }
#pragma warning restore CA1822 // Mark members as static
    }

    public sealed class NullCache : IMemoryCache
    {
        public ICacheEntry CreateEntry(object key)
        {
            return new NullCacheEntry();
        }

        public void Remove(object key)
        {
        }

        public bool TryGetValue(object key, out object value)
        {
            value = default;
            return false;
        }

        public void Dispose()
        {
        }

        private sealed class NullCacheEntry : ICacheEntry
        {
            public DateTimeOffset? AbsoluteExpiration { get; set; }
            public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
            public IList<IChangeToken> ExpirationTokens { get; set; }
            public object Key { get; set; }
            public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; set; }
            public CacheItemPriority Priority { get; set; }
            public long? Size { get; set; }
            public TimeSpan? SlidingExpiration { get; set; }
            public object Value { get; set; }

            public void Dispose()
            {
            }
        }
    }
}

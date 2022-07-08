using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using OrchardCore.Abstractions.Pooling;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.TagHelpers
{
    [HtmlTargetElement("dynamic-cache", Attributes = CacheIdAttributeName)]
    public class DynamicCacheTagHelper : TagHelper
    {
        private const string CacheIdAttributeName = "cache-id";
        private const string VaryByAttributeName = "vary-by";
        private const string DependenciesAttributeNAme = "dependencies";
        private const string ExpiresOnAttributeName = "expires-on";
        private const string ExpiresAfterAttributeName = "expires-after";
        private const string ExpiresSlidingAttributeName = "expires-sliding";
        private const string EnabledAttributeName = "enabled";

        private static readonly char[] SplitChars = new[] { ',', ' ' };

        /// <summary>
        /// The default duration, from the time the cache entry was added, when it should be evicted.
        /// This default duration will only be used if no other expiration criteria is specified.
        /// The default expiration time is a sliding expiration of 30 seconds.
        /// </summary>
        public static readonly TimeSpan DefaultExpiration = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets the <see cref="System.Text.Encodings.Web.HtmlEncoder"/> which encodes the content to be cached.
        /// </summary>
        protected HtmlEncoder HtmlEncoder { get; }

        /// <summary>
        /// Gets or sets a <see cref="string" /> identifying this cache entry.
        /// </summary>
        [HtmlAttributeName(CacheIdAttributeName)]
        public string CacheId { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="string" /> to vary the cached result by.
        /// </summary>
        [HtmlAttributeName(VaryByAttributeName)]
        public string VaryBy { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="string" /> with the dependencies to invalidate the cache with.
        /// </summary>
        [HtmlAttributeName(DependenciesAttributeNAme)]
        public string Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the exact <see cref="DateTimeOffset"/> the cache entry should be evicted.
        /// </summary>
        [HtmlAttributeName(ExpiresOnAttributeName)]
        public DateTimeOffset? ExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets the duration, from the time the cache entry was added, when it should be evicted.
        /// </summary>
        [HtmlAttributeName(ExpiresAfterAttributeName)]
        public TimeSpan? ExpiresAfter { get; set; }

        /// <summary>
        /// Gets or sets the duration from last access that the cache entry should be evicted.
        /// </summary>
        [HtmlAttributeName(ExpiresSlidingAttributeName)]
        public TimeSpan? ExpiresSliding { get; set; }

        /// <summary>
        /// Gets or sets the value which determines if the tag helper is enabled or not.
        /// </summary>
        [HtmlAttributeName(EnabledAttributeName)]
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Prefix used by <see cref="DynamicCacheTagHelper"/> instances when creating entries in <see cref="IDynamicCacheService"/>.
        /// </summary>
        public static readonly string CacheKeyPrefix = nameof(DynamicCacheTagHelper);

        private readonly IDynamicCacheService _dynamicCacheService;
        private readonly ICacheScopeManager _cacheScopeManager;
        private readonly DynamicCacheTagHelperService _dynamicCacheTagHelperService;
        private readonly CacheOptions _cacheOptions;

        public DynamicCacheTagHelper(
            IDynamicCacheService dynamicCacheService,
            ICacheScopeManager cacheScopeManager,
            HtmlEncoder htmlEncoder,
            DynamicCacheTagHelperService dynamicCacheTagHelperService,
            IOptions<CacheOptions> cacheOptions)
        {
            _dynamicCacheService = dynamicCacheService;
            _cacheScopeManager = cacheScopeManager;
            HtmlEncoder = htmlEncoder;
            _dynamicCacheTagHelperService = dynamicCacheTagHelperService;
            _cacheOptions = cacheOptions.Value;
        }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            IHtmlContent content;

            if (Enabled)
            {
                var cacheContext = new CacheContext(CacheId);

                if (!String.IsNullOrEmpty(VaryBy))
                {
                    cacheContext.AddContext(VaryBy.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries));
                }

                if (!String.IsNullOrEmpty(Dependencies))
                {
                    cacheContext.AddTag(Dependencies.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries));
                }

                var hasEvictionCriteria = false;

                if (ExpiresOn.HasValue)
                {
                    hasEvictionCriteria = true;
                    cacheContext.WithExpiryOn(ExpiresOn.Value);
                }

                if (ExpiresAfter.HasValue)
                {
                    hasEvictionCriteria = true;
                    cacheContext.WithExpiryAfter(ExpiresAfter.Value);
                }

                if (ExpiresSliding.HasValue)
                {
                    hasEvictionCriteria = true;
                    cacheContext.WithExpirySliding(ExpiresSliding.Value);
                }

                if (!hasEvictionCriteria)
                {
                    cacheContext.WithExpirySliding(DefaultExpiration);
                }

                _cacheScopeManager.EnterScope(cacheContext);

                try
                {
                    content = await ProcessContentAsync(output, cacheContext);
                }
                finally
                {
                    _cacheScopeManager.ExitScope();
                }
            }
            else
            {
                content = await output.GetChildContentAsync();
            }

            // Clear the contents of the "cache" element since we don't want to render it.
            output.SuppressOutput();

            output.Content.SetHtmlContent(content);
        }

        public async Task<IHtmlContent> ProcessContentAsync(TagHelperOutput output, CacheContext cacheContext)
        {
            IHtmlContent content = null;

            while (content == null)
            {
                Task<IHtmlContent> result;

                // Is there any request already processing the value?
                if (!_dynamicCacheTagHelperService.Workers.TryGetValue(CacheId, out result))
                {
                    // There is a small race condition here between TryGetValue and TryAdd that might cause the
                    // content to be computed more than once. We don't care about this race as the probability of
                    // happening is very small and the impact is not critical.
                    var tcs = new TaskCompletionSource<IHtmlContent>();

                    _dynamicCacheTagHelperService.Workers.TryAdd(CacheId, tcs.Task);

                    try
                    {
                        var value = await _dynamicCacheService.GetCachedValueAsync(cacheContext);

                        if (value == null)
                        {
                            // The value is not cached, we need to render the tag helper output
                            var processedContent = await output.GetChildContentAsync();

                            using var writer = new ZStringWriter();
                            // Write the start of a cache debug block.
                            if (_cacheOptions.DebugMode)
                            {
                                // No need to optimize this code as it will be used for debugging purpose.
                                writer.WriteLine();
                                writer.WriteLine($"<!-- CACHE BLOCK: {cacheContext.CacheId} ({Guid.NewGuid()})");
                                writer.WriteLine($"         VARY BY: {String.Join(", ", cacheContext.Contexts)}");
                                writer.WriteLine($"    DEPENDENCIES: {String.Join(", ", cacheContext.Tags)}");
                                writer.WriteLine($"      EXPIRES ON: {cacheContext.ExpiresOn}");
                                writer.WriteLine($"   EXPIRES AFTER: {cacheContext.ExpiresAfter}");
                                writer.WriteLine($" EXPIRES SLIDING: {cacheContext.ExpiresSliding}");
                                writer.WriteLine("-->");
                            }

                            // Always write the content regardless of debug mode.
                            processedContent.WriteTo(writer, HtmlEncoder);

                            // Write the end of a cache debug block.
                            if (_cacheOptions.DebugMode)
                            {
                                writer.WriteLine();
                                writer.WriteLine($"<!-- END CACHE BLOCK: {cacheContext.CacheId} -->");
                            }

                            await writer.FlushAsync();

                            var html = writer.ToString();

                            var formattingContext = new DistributedCacheTagHelperFormattingContext
                            {
                                Html = new HtmlString(html)
                            };

                            await _dynamicCacheService.SetCachedValueAsync(cacheContext, html);

                            content = formattingContext.Html;
                        }
                        else
                        {
                            content = new HtmlString(value);
                        }
                    }
                    catch
                    {
                        content = null;
                        throw;
                    }
                    finally
                    {
                        // Remove the worker task before setting the result.
                        // If the result is null, other threads would potentially
                        // acquire it otherwise.
                        _dynamicCacheTagHelperService.Workers.TryRemove(CacheId, out result);

                        // Notify all other awaiters to render the content
                        tcs.TrySetResult(content);
                    }
                }
                else
                {
                    content = await result;
                }
            }

            return content;
        }
    }
}

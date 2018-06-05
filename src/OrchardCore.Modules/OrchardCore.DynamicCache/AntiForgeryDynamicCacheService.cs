using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.DynamicCache.Services;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache
{
    public class AntiForgeryDynamicCacheService : IDynamicCacheService
    {
        private const string Placeholder = "#{AntiForgeryToken}";

        private readonly IDynamicCacheService _dynamicCacheService;
        private readonly Regex _tagRegex;
        private readonly Lazy<string> _tagFactory; // This ensures that we only generate the markup once per request

        public AntiForgeryDynamicCacheService(
            IDynamicCacheService dynamicCacheService, 
            IAntiforgery antiforgery,
            IHttpContextAccessor httpContextAccessor)
        {
            _dynamicCacheService = dynamicCacheService;

            _tagRegex = new Regex("<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"[^\"]*\" />");
            _tagFactory = new Lazy<string>(() =>
            {
                var htmlContent = antiforgery.GetHtml(httpContextAccessor.HttpContext);

                using (var writer = new StringWriter())
                {
                    htmlContent.WriteTo(writer, HtmlEncoder.Default);
                    return writer.ToString();
                }
            });
        }

        public async Task<string> GetCachedValueAsync(CacheContext context)
        {
            var cachedValue = await _dynamicCacheService.GetCachedValueAsync(context);

            return ReplacePlaceholderWithTag(cachedValue);
        }

        public async Task SetCachedValueAsync(CacheContext context, string value)
        {
            await _dynamicCacheService.SetCachedValueAsync(context, ReplaceTagWithPlaceholder(value));
        }

        private string ReplaceTagWithPlaceholder(string value)
        {
            return _tagRegex.Replace(value, Placeholder);
        }

        private string ReplacePlaceholderWithTag(string value)
        {
            // Don't bother if this was a cache miss or if there's no placeholders to be replaced
            if (value != null && value.Contains(Placeholder))
            {
                value = value.Replace(Placeholder, _tagFactory.Value);
            }

            return value;
        }
    }
}
using System.IO.Hashing;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

#nullable enable

namespace OrchardCore.Facebook.Endpoints;

public static class GetSdkEndpoints
{
    public static IEndpointRouteBuilder AddSdkEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/OrchardCore.Facebook/sdk/init.js", GetInitScriptEndpoint.HandleRequestAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        builder.MapGet("/OrchardCore.Facebook/sdk/sdk.{culture:length(2,6)}.js", GetFetchScriptEndpoint.HandleRequestAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    public static class GetInitScriptEndpoint
    {
        // Update this version when the script changes to invalidate client caches
        private static readonly int ScriptVersion = 1;

        private static readonly string CacheKey = "Facebook.GetInitScriptEndpoint";

        public static ulong HashCacheBustingValues(FacebookSettings settings)
        {
            var hash = new XxHash3(ScriptVersion);
            hash.Append(Encoding.UTF8.GetBytes(settings.AppId ?? ""));
            hash.Append(Encoding.UTF8.GetBytes(settings.Version ?? ""));
            hash.Append(Encoding.UTF8.GetBytes(settings.FBInitParams ?? ""));
            return hash.GetCurrentHashAsUInt64();
        }

        public static async Task<IResult> HandleRequestAsync(HttpContext context, ISiteService siteService, IMemoryCache cache)
        {
            byte[] scriptBytes;
            var settings = await siteService.GetSettingsAsync<FacebookSettings>();
            // Regenerate hash: Don't trust url hash because it could cause cache issues
            var expectedHash = HashCacheBustingValues(settings);

            var cachedScript = cache.Get(CacheKey) as KeyValuePair<ulong, byte[]>?;
            if (cachedScript == null || cachedScript.Value.Key != expectedHash)
            {
                // Note: Update ScriptVersion constant when the script changes
                // Note: All injected values except those in url must be used in HashCacheBustingValues
                scriptBytes = Encoding.UTF8.GetBytes($@"
                    window.fbAsyncInit = function() {{
                        FB.init({{
                            appId:'{settings.AppId}',
                            version:'{settings.Version}',
                            {settings.FBInitParams}
                        }});
                    }};");

                cache.Set(CacheKey,
                        new KeyValuePair<ulong, byte[]> (expectedHash, scriptBytes),
                        new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));
            }
            else
            {
                scriptBytes = cachedScript.Value.Value;
            }

            // Set the cache timeout to the maximum allowed length of one year
            // max-age is needed because immutable is not widely supported
            context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";

            return Results.Bytes(scriptBytes, "application/javascript");
        }
    }

    public static class GetFetchScriptEndpoint
    {
        // Update this version when the script changes to invalidate client caches
        private static readonly int ScriptVersion = 1;

        private static readonly string CacheKey = "Facebook.GetFetchScriptEndpoint";

        // Scraped from facebook.com
        public static readonly string[] ValidFacebookCultures = { "en-US", "es-LA", "pt-BR", "fr-FR", "de-DE", "so-SO", "af-ZA", "az-AZ", "id-ID", "ms-MY", "jv-ID", "cx-PH", "bs-BA", "br-FR", "ca-ES", "cs-CZ", "co-FR", "cy-GB", "da-DK", "de-DE", "et-EE", "en-GB", "en-US", "es-LA", "es-ES", "eo-EO", "eu-ES", "tl-PH", "fo-FO", "fr-CA", "fr-FR", "fy-NL", "ff-NG", "fn-IT", "ga-IE", "gl-ES", "gn-PY", "ha-NG", "hr-HR", "rw-RW", "iu-CA", "ik-US", "is-IS", "it-IT", "sw-KE", "ht-HT", "ku-TR", "lv-LV", "lt-LT", "hu-HU", "mg-MG", "mt-MT", "nl-NL", "nb-NO", "nn-NO", "uz-UZ", "pl-PL", "pt-BR", "pt-PT", "ro-RO", "sc-IT", "sn-ZW", "sq-AL", "sz-PL", "sk-SK", "sl-SI", "fi-FI", "sv-SE", "vi-VN", "tr-TR", "nl-BE", "zz-TR", "el-GR", "be-BY", "bg-BG", "ky-KG", "kk-KZ", "mk-MK", "mn-MN", "ru-RU", "sr-RS", "tt-RU", "tg-TJ", "uk-UA", "ka-GE", "hy-AM", "he-IL", "ur-PK", "ar-AR", "ps-AF", "fa-IR", "cb-IQ", "sy-SY", "tz-MA", "am-ET", "ne-NP", "mr-IN", "hi-IN", "as-IN", "bn-IN", "pa-IN", "gu-IN", "or-IN", "ta-IN", "te-IN", "kn-IN", "ml-IN", "si-LK", "th-TH", "lo-LA", "my-MM", "km-KH", "ko-KR", "zh-TW", "zh-CN", "zh-HK", "ja-JP", "ja-KS" };

        public static ulong HashCacheBustingValues(FacebookSettings settings)
        {
            var hash = new XxHash3(ScriptVersion);
            hash.Append(Encoding.UTF8.GetBytes(settings.SdkJs ?? ""));
            return hash.GetCurrentHashAsUInt64();
        }

        public static async Task<IResult> HandleRequestAsync(string culture, HttpContext context, IMemoryCache cache, UrlEncoder urlEncoder, ISiteService siteService)
        {
            byte[] scriptBytes;
            var settings = await siteService.GetSettingsAsync<FacebookSettings>();
            // Regenerate hash: Don't trust url hash because it could cause cache issues
            var expectedHash = HashCacheBustingValues(settings);

            var cachedScript = cache.Get(CacheKey) as KeyValuePair<ulong, byte[]>?;
            if (cachedScript == null || cachedScript.Value.Key != expectedHash)
            {
                var encodedCulture = urlEncoder.Encode(culture.Replace('-', '_'));

                // Note: If a culture is not found, facebook will use en_US
                // Note: Update ScriptVersion constant when the script changes
                // Note: All injected values except those in url must be used in HashCacheBustingValues
                scriptBytes = Encoding.UTF8.GetBytes($@"(function(d){{
                    var js, id = 'facebook-jssdk'; if (d.getElementById(id)) {{ return; }}
                    js = d.createElement('script'); js.id = id; js.async = true;
                    js.src = ""https://connect.facebook.net/{encodedCulture}/{settings.SdkJs}"";
                    d.getElementsByTagName('head')[0].appendChild(js);
                }} (document));");

                cache.Set(CacheKey,
                        new KeyValuePair<ulong, byte[]> (expectedHash, scriptBytes),
                        new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));
            }
            else
            {
                scriptBytes = cachedScript.Value.Value;
            }

            // Set the cache timeout to the maximum allowed length of one year
            // max-age is needed because immutable is not widely supported
            context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";

            return Results.Bytes(scriptBytes, "application/javascript");
        }
    }
}

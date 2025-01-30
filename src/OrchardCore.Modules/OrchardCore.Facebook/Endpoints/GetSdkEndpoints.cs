using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

#nullable enable

namespace OrchardCore.Facebook.Endpoints;

public static class GetSdkEndpoints
{
    // Update this version when the script changes to invalidate client caches
    public static readonly int Version = 1;

    // Scraped from facebook.com
    public static readonly string[] ValidFacebookCultures = { "en-US", "es-LA", "pt-BR", "fr-FR", "de-DE", "so-SO", "af-ZA", "az-AZ", "id-ID", "ms-MY", "jv-ID", "cx-PH", "bs-BA", "br-FR", "ca-ES", "cs-CZ", "co-FR", "cy-GB", "da-DK", "de-DE", "et-EE", "en-GB", "en-US", "es-LA", "es-ES", "eo-EO", "eu-ES", "tl-PH", "fo-FO", "fr-CA", "fr-FR", "fy-NL", "ff-NG", "fn-IT", "ga-IE", "gl-ES", "gn-PY", "ha-NG", "hr-HR", "rw-RW", "iu-CA", "ik-US", "is-IS", "it-IT", "sw-KE", "ht-HT", "ku-TR", "lv-LV", "lt-LT", "hu-HU", "mg-MG", "mt-MT", "nl-NL", "nb-NO", "nn-NO", "uz-UZ", "pl-PL", "pt-BR", "pt-PT", "ro-RO", "sc-IT", "sn-ZW", "sq-AL", "sz-PL", "sk-SK", "sl-SI", "fi-FI", "sv-SE", "vi-VN", "tr-TR", "nl-BE", "zz-TR", "el-GR", "be-BY", "bg-BG", "ky-KG", "kk-KZ", "mk-MK", "mn-MN", "ru-RU", "sr-RS", "tt-RU", "tg-TJ", "uk-UA", "ka-GE", "hy-AM", "he-IL", "ur-PK", "ar-AR", "ps-AF", "fa-IR", "cb-IQ", "sy-SY", "tz-MA", "am-ET", "ne-NP", "mr-IN", "hi-IN", "as-IN", "bn-IN", "pa-IN", "gu-IN", "or-IN", "ta-IN", "te-IN", "kn-IN", "ml-IN", "si-LK", "th-TH", "lo-LA", "my-MM", "km-KH", "ko-KR", "zh-TW", "zh-CN", "zh-HK", "ja-JP", "ja-KS" };

    public static IEndpointRouteBuilder AddSdkEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/OrchardCore.Facebook/sdk/{hash}/init.js", HandleInitScriptRequestAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        builder.MapGet("/OrchardCore.Facebook/sdk/{hash}/sdk.{culture:length(2,6)}.js", HandleFetchScriptRequestAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    private static async Task<IResult> HandleFetchScriptRequestAsync(string hash, string culture, HttpContext context, IMemoryCache cache, UrlEncoder urlEncoder, ISiteService siteService)
    {
        var settings = await siteService.GetSettingsAsync<FacebookSettings>();

        if (hash != settings.GetHash().ToString())
        {
            return Results.NotFound();
        }

        var scriptCacheKey = $"/OrchardCore.Facebook/sdk/{hash}/sdk.{culture}.js";

        var scriptBytes = await cache.GetOrCreateAsync(scriptCacheKey, async entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromDays(1));

            var encodedCulture = urlEncoder.Encode(culture.Replace('-', '_'));

            // Note: If a culture is not found, facebook will use en_US
            // Note: Update Version constant when the script changes
            return Encoding.UTF8.GetBytes($@"(function(d){{
                var js, id = 'facebook-jssdk'; if (d.getElementById(id)) {{ return; }}
                js = d.createElement('script'); js.id = id; js.async = true;
                js.src = ""https://connect.facebook.net/{encodedCulture}/{settings.SdkJs}"";
                d.getElementsByTagName('head')[0].appendChild(js);
            }} (document));");
        });

        if (scriptBytes == null)
        {
            return Results.NotFound();
        }

        // Set the cache timeout to the maximum allowed length of one year
        // max-age is needed because immutable is not widely supported
        context.Response.Headers.CacheControl = $"public, max-age=31536000, immutable";

        return Results.Bytes(scriptBytes, "application/javascript");
    }

    private static async Task<IResult> HandleInitScriptRequestAsync(string hash, HttpContext context, ISiteService siteService, IMemoryCache cache, IShellConfiguration shellConfiguration)
    {
        var settings = await siteService.GetSettingsAsync<FacebookSettings>();

        if (hash != settings.GetHash().ToString())
        {
            return Results.NotFound();
        }

        var scriptCacheKey = $"/OrchardCore.Facebook/sdk/fb.js/{hash}/init.js";

        var scriptBytes = cache.GetOrCreate(scriptCacheKey, entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromHours(1));

            return Encoding.UTF8.GetBytes($@"
                window.fbAsyncInit = function() {{
                    FB.init({{
                        appId:'{settings.AppId}',
                        version:'{settings.Version}',
                        {settings.FBInitParams}
                    }});
                }};");
        });

        if (scriptBytes == null)
        {
            return Results.NotFound();
        }

        // Set the cache timeout to the maximum allowed length of one year
        // max-age is needed because immutable is not widely supported
        context.Response.Headers.CacheControl = $"public, max-age=31536000, immutable";

        return Results.Bytes(scriptBytes, "application/javascript");
    }
}

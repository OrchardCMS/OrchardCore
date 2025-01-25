using System.IO.Hashing;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

#nullable enable

namespace OrchardCore.Facebook.Endpoints;

public static class GetSdkEndpoints
{
    // Scraped from facebook.com
    public static readonly string[] ValidFacebookCultures = { "en-US", "es-LA", "pt-BR", "fr-FR", "de-DE", "so-SO", "af-ZA", "az-AZ", "id-ID", "ms-MY", "jv-ID", "cx-PH", "bs-BA", "br-FR", "ca-ES", "cs-CZ", "co-FR", "cy-GB", "da-DK", "de-DE", "et-EE", "en-GB", "en-US", "es-LA", "es-ES", "eo-EO", "eu-ES", "tl-PH", "fo-FO", "fr-CA", "fr-FR", "fy-NL", "ff-NG", "fn-IT", "ga-IE", "gl-ES", "gn-PY", "ha-NG", "hr-HR", "rw-RW", "iu-CA", "ik-US", "is-IS", "it-IT", "sw-KE", "ht-HT", "ku-TR", "lv-LV", "lt-LT", "hu-HU", "mg-MG", "mt-MT", "nl-NL", "nb-NO", "nn-NO", "uz-UZ", "pl-PL", "pt-BR", "pt-PT", "ro-RO", "sc-IT", "sn-ZW", "sq-AL", "sz-PL", "sk-SK", "sl-SI", "fi-FI", "sv-SE", "vi-VN", "tr-TR", "nl-BE", "zz-TR", "el-GR", "be-BY", "bg-BG", "ky-KG", "kk-KZ", "mk-MK", "mn-MN", "ru-RU", "sr-RS", "tt-RU", "tg-TJ", "uk-UA", "ka-GE", "hy-AM", "he-IL", "ur-PK", "ar-AR", "ps-AF", "fa-IR", "cb-IQ", "sy-SY", "tz-MA", "am-ET", "ne-NP", "mr-IN", "hi-IN", "as-IN", "bn-IN", "pa-IN", "gu-IN", "or-IN", "ta-IN", "te-IN", "kn-IN", "ml-IN", "si-LK", "th-TH", "lo-LA", "my-MM", "km-KH", "ko-KR", "zh-TW", "zh-CN", "zh-HK", "ja-JP", "ja-KS" };

    public static IEndpointRouteBuilder AddSdkEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/OrchardCore.Facebook/sdk/init.js", HandleInitScriptRequestAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        builder.MapGet("/OrchardCore.Facebook/sdk/fetch_{toFetch:length(1,40)}.js", HandleFetchScriptRequestAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    private static IResult HandleFetchScriptRequestAsync(string toFetch, HttpContext context, IMemoryCache cache, UrlEncoder urlEncoder)
    {
        var results = toFetch.Split('.', 2);

        string stem = "sdk";
        if (results.Length > 0 && results[0] != "")
        {
            stem = results[0];
        }

        string culture = "en-US";
        if (results.Length > 1 && results[1] != "")
        {
            culture = results[1];
        }

        // Set the cache timeout to the maximum allowed length of one year
        // max-age is needed because immutable is not widely supported
        context.Response.Headers.CacheControl = $"public, max-age=31536000, immutable";

        var scriptCacheKey = $"/OrchardCore.Facebook/sdk/fetch_{stem}.{culture}.js";

        var scriptBytes = cache.Get(scriptCacheKey) as byte[];
        if (scriptBytes == null)
        {
            var encodedCulture = urlEncoder.Encode(culture.Replace('-', '_'));
            var encodedStem = urlEncoder.Encode(stem);

            // Note: If a culture is not found, facebook will use en_US
            // Note: Update script version in ResourceManagementOptionsConfiguration.cs after editing
            scriptBytes = Encoding.UTF8.GetBytes($@"(function(d){{
                var js, id = 'facebook-jssdk'; if (d.getElementById(id)) {{ return; }}
                js = d.createElement('script'); js.id = id; js.async = true;
                js.src = ""https://connect.facebook.net/{encodedCulture}/{encodedStem}.js"";
                d.getElementsByTagName('head')[0].appendChild(js);
            }} (document));");

            cache.Set(scriptCacheKey, scriptBytes);
        }

        return Results.Bytes(scriptBytes, "application/javascript");
    }

    private static async Task<IResult> HandleInitScriptRequestAsync(HttpContext context, ISiteService siteService)
    {
        var settings = await siteService.GetSettingsAsync<FacebookSettings>();

        if (string.IsNullOrWhiteSpace(settings.AppId))
        {
            return Results.Forbid();
        }

        string options = $"{{ appId:'{settings.AppId}',version:'{settings.Version}'";
        if (!string.IsNullOrWhiteSpace(settings.FBInitParams))
        {
            options += "," + settings.FBInitParams;
        }
        options += "}";

        var scriptBytes = (new byte[][] {
                "window.fbAsyncInit = function(){ FB.init("u8.ToArray(),
                Encoding.UTF8.GetBytes(options),
                ");};"u8.ToArray(),
            }).SelectMany(x => x).ToArray();

        // Uses cross-processes hashing to enable revalidation after restart
        StringValues eTag = $"\"{XxHash3.HashToUInt64(scriptBytes, 0)}\"";

        context.Response.Headers.CacheControl = "no-cache";
        context.Response.Headers.ETag = eTag;

        // Assumes IfNoneMatch has only one ETag for performance
        if (context.Request.Headers.IfNoneMatch.Equals(eTag))
        {
            return Results.StatusCode(304);
        }

        return Results.Bytes(scriptBytes, "application/javascript");
    }
}

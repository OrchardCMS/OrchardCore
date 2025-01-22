using System.IO.Hashing;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

#nullable enable

namespace OrchardCore.Facebook.Endpoints;

public static class GetSdkEndpoints
{
    public static IEndpointRouteBuilder AddSdkEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/OrchardCore.Facebook/sdk/fbsdk.js", HandleFbsdkScriptRequestAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        builder.MapGet("/OrchardCore.Facebook/sdk/fb.js", HandleFbScriptRequestAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    private static IResult HandleFbsdkScriptRequestAsync([FromQuery(Name = "lang")] string language, [FromQuery(Name = "sdkf")] string sdkFilename, HttpContext context, IMemoryCache cache)
    {
        // Set the cache timeout to the maximum allowed length of one year
        // max-age is needed because immutable is not widly supported
        context.Response.Headers.CacheControl = $"public, max-age=31536000, immutable";

        string scriptCacheKey = $"~/OrchardCore.Facebook/sdk/fbsdk.js?sdkf={sdkFilename}&lang={language}";

        var scriptBytes = cache.Get(scriptCacheKey) as byte[];
        if (scriptBytes == null)
        {
            var encodedLanguage = WebUtility.UrlEncode(language.Replace('-', '_'));
            var encodedFilename = WebUtility.UrlEncode(sdkFilename);

            // Note: Update script version in ResourceManagementOptionsConfiguration.cs after editing
            scriptBytes = Encoding.UTF8.GetBytes($@"(function(d){{
                var js, id = 'facebook-jssdk'; if (d.getElementById(id)) {{ return; }}
                js = d.createElement('script'); js.id = id; js.async = true;
                js.src = ""https://connect.facebook.net/{encodedLanguage}/{encodedFilename}"";
                d.getElementsByTagName('head')[0].appendChild(js);
            }} (document));");

            cache.Set(scriptCacheKey, scriptBytes);
        }

        return Results.Bytes(scriptBytes, "application/javascript");
    }

    private static async Task<IResult> HandleFbScriptRequestAsync(HttpContext context, ISiteService siteService, IMemoryCache cache, IShellConfiguration shellConfiguration)
    {
        var settings = await siteService.GetSettingsAsync<FacebookSettings>();

        if (string.IsNullOrWhiteSpace(settings.AppId))
        {
            return Results.Forbid();
        }

        context.Response.Headers.CacheControl = shellConfiguration.GetValue(
                "StaticFileOptions:CacheControl",
                // Fallback value
                $"public, max-age={TimeSpan.FromDays(30).TotalSeconds}");

        // Assumes IfNoneMatch has only one ETag for performance
        var requestETag = context.Request.Headers.IfNoneMatch;
        if (!StringValues.IsNullOrEmpty(requestETag) && cache.Get(requestETag) != null)
        {
            context.Response.Headers.ETag = requestETag;

            return Results.StatusCode(304);
        }

        string scriptCacheKey = $"/OrchardCore.Facebook/sdk/fb.js.{settings.AppId}.{settings.Version}";

        var scriptBytes = (byte[]?)cache.Get(scriptCacheKey);
        if (scriptBytes == null)
        {
            // Generate script
            var options = $"{{ appId:'{settings.AppId}',version:'{settings.Version}'";
            options = string.IsNullOrWhiteSpace(settings.FBInitParams)
                ? string.Concat(options, "}")
                : string.Concat(options, ",", settings.FBInitParams, "}");
            scriptBytes = Encoding.UTF8.GetBytes($"window.fbAsyncInit = function(){{ FB.init({options});}};");

            cache.Set(scriptCacheKey, scriptBytes);
        }

        // Uses cross-processes hashing to enable revalidation after restart
        StringValues eTag = $"\"{XxHash3.HashToUInt64(scriptBytes, 0)}\"";

        // Mark that the eTag corresponds to a fresh file
        cache.Set(eTag, true);

        context.Response.Headers.ETag = eTag;

        // Can be true if the cache was reset after the last client request
        if (requestETag.Equals(eTag))
        {
            return Results.StatusCode(304);
        }

        return Results.Bytes(scriptBytes, "application/javascript");
    }
}

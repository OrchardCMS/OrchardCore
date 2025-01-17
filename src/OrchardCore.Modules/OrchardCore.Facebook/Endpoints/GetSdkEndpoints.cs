using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;
using OrchardCore.Facebook.Settings;
using System.Globalization;
using System.Text;

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

    private static uint GetHashCode(byte[] bytes)
    {
        uint hash = 0;
        foreach (byte b in bytes)
        {
            hash += b;
            hash *= 17;
        }
        return hash;
    }

    private static async Task<IResult> HandleFbsdkScriptRequestAsync(HttpContext context, ISiteService siteService, IMemoryCache cache)
    {
        context.Response.Headers.Vary = "Accept-Language";
        var shellConfiguration = context.RequestServices.GetRequiredService<IShellConfiguration>();
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

        var settings = await siteService.GetSettingsAsync<FacebookSettings>();
        var locale = CultureInfo.CurrentUICulture.Name;
        string scriptCacheKey = $"/OrchardCore.Facebook/sdk/fbsdk.js.{locale}.{settings.SdkJs}";

        var scriptBytes = (byte[]?)cache.Get(scriptCacheKey);
        if (scriptBytes == null)
        {
            scriptBytes = Encoding.UTF8.GetBytes($@"(function(d){{
                var js, id = 'facebook-jssdk'; if (d.getElementById(id)) {{ return; }}
                js = d.createElement('script'); js.id = id; js.async = true;
                js.src = ""https://connect.facebook.net/{locale.Replace('-', '_')}/{settings.SdkJs}"";
                d.getElementsByTagName('head')[0].appendChild(js);
            }} (document));");

            cache.Set(scriptCacheKey, scriptBytes);
        }

// False positive: No comparison is taking place here
#pragma warning disable RS1024
        // Uses a custom GetHashCode because Object.GetHashCode differs across processes
        StringValues eTag = $"\"{GetHashCode(scriptBytes)}\"";
#pragma warning restore RS1024

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

    private static async Task<IResult> HandleFbScriptRequestAsync(HttpContext context, ISiteService siteService, IMemoryCache cache)
    {
        var settings = await siteService.GetSettingsAsync<FacebookSettings>();

        if (string.IsNullOrWhiteSpace(settings.AppId))
        {
            return Results.Forbid();
        }

        var shellConfiguration = context.RequestServices.GetRequiredService<IShellConfiguration>();
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

        string scriptCacheKey = $"/OrchardCore.Facebook/sdk/fbsdk.js.{settings.AppId}.{settings.Version}";

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

        // Uses a custom GetHashCode because Object.GetHashCode differs across processes

// False positive: No comparison is taking place here
#pragma warning disable RS1024
        StringValues eTag = $"\"{GetHashCode(scriptBytes)}\"";
#pragma warning restore RS1024

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

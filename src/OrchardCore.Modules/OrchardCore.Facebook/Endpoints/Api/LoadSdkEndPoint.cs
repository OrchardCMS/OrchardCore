using System.Globalization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.Settings;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook;
internal static class LoadSdkEndpoint
{
    public static IEndpointRouteBuilder AddLoadSdkEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("orchardcore/facebook/sdk/fb.js", HandleAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> HandleAsync(
        ContentItem model,
        ISiteService siteService,
        HttpContext httpContext,
        bool draft = false)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var script = await getLoadSdkScript(siteService);

        if (script == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(script);
    }

    private static async Task<string> getLoadSdkScript(ISiteService siteService)
    {
        var settings = await siteService.GetSettingsAsync<FacebookSettings>();
        var locale = CultureInfo.CurrentUICulture.Name.Replace('-', '_');

        var script = default(string);

        if (!string.IsNullOrWhiteSpace(settings?.AppId))
        {
            var options = $"{{ appId:'{settings.AppId}',version:'{settings.Version}'";
            options = string.IsNullOrWhiteSpace(settings.FBInitParams)
                ? string.Concat(options, "}")
                : string.Concat(options, ",", settings.FBInitParams, "}");

            script = $"window.fbAsyncInit = function(){{ FB.init({options});}};";
        }

        return script;
    }
}


using System.Globalization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.Settings;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook;
internal static class InitSdkEndpoint
{
    public static IEndpointRouteBuilder AddInitSdkEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("orchardcore/facebook/sdk/fbsdk.js", HandleAsync)
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

        var script = await getInitSdkScript(siteService);

        if (script == null)
        {
            return Results.NotFound(); 
        }

        return Results.Ok(script); 
    }

    private static async Task<string> getInitSdkScript(ISiteService siteService)
    {
        var settings = await siteService.GetSettingsAsync<FacebookSettings>();
        var locale = CultureInfo.CurrentUICulture.Name.Replace('-', '_');

        var script = $@"(function(d){{
                    var js, id = 'facebook-jssdk'; if (d.getElementById(id)) {{ return; }}
                    js = d.createElement('script'); js.id = id; js.async = true;
                    js.src = ""https://connect.facebook.net/{locale}/{settings.SdkJs}"";
                    d.getElementsByTagName('head')[0].appendChild(js);
                }} (document));";

        return script;
    }
}


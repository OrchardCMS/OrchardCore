using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Facebook;

public class ScriptsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISiteService _siteService;

    public ScriptsMiddleware(RequestDelegate next, ISiteService siteService)
    {
        _next = next;
        _siteService = siteService;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (httpContext.Request.Path.StartsWithSegments("/OrchardCore.Facebook/sdk", StringComparison.OrdinalIgnoreCase))
        {
            var script = default(string);
            var settings = await _siteService.GetSettingsAsync<FacebookSettings>();

            if (Path.GetFileName(httpContext.Request.Path.Value) == "fbsdk.js")
            {
                var locale = CultureInfo.CurrentUICulture.Name.Replace('-', '_');
                script = $@"(function(d){{
                        var js, id = 'facebook-jssdk'; if (d.getElementById(id)) {{ return; }}
                        js = d.createElement('script'); js.id = id; js.async = true;
                        js.src = ""https://connect.facebook.net/{locale}/{settings.SdkJs}"";
                        d.getElementsByTagName('head')[0].appendChild(js);
                    }} (document));";
            }
            else if (Path.GetFileName(httpContext.Request.Path.Value) == "fb.js")
            {
                if (!string.IsNullOrWhiteSpace(settings?.AppId))
                {
                    var options = $"{{ appId:'{settings.AppId}',version:'{settings.Version}'";
                    options = string.IsNullOrWhiteSpace(settings.FBInitParams)
                        ? string.Concat(options, "}")
                        : string.Concat(options, ",", settings.FBInitParams, "}");

                    script = $"window.fbAsyncInit = function(){{ FB.init({options});}};";
                }
            }

            if (script != null)
            {
                var bytes = Encoding.UTF8.GetBytes(script);
                await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(script).AsMemory(0, bytes.Length), httpContext.RequestAborted);

                return;
            }
        }

        await _next.Invoke(httpContext);
    }
}

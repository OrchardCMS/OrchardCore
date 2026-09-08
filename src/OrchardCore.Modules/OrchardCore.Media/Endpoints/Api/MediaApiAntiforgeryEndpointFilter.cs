using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Settings;

namespace OrchardCore.Media.Endpoints.Api;

/// <summary>
/// Validates the antiforgery token when the Media API is in cookie mode. In bearer mode there is no
/// ambient cookie authentication, so there is no CSRF surface and validation is skipped (the
/// endpoints also <c>DisableAntiforgery()</c> so the framework never validates on its own).
/// </summary>
public sealed class MediaApiAntiforgeryEndpointFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var settings = httpContext.RequestServices.GetRequiredService<ISiteService>().GetSettings<MediaApiSettings>();

        if (settings.AuthenticationScheme == MediaApiAuthenticationScheme.Cookie)
        {
            var antiforgery = httpContext.RequestServices.GetRequiredService<IAntiforgery>();

            try
            {
                await antiforgery.ValidateRequestAsync(httpContext);
            }
            catch (AntiforgeryValidationException)
            {
                return TypedResults.Problem(
                    detail: "The antiforgery token was missing or invalid.",
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }

        return await next(context);
    }
}

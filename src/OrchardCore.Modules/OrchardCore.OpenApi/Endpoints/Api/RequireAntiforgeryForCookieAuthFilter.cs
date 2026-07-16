using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.OpenApi.Endpoints.Api;

/// <summary>
/// The <see cref="OpenApiAuthenticationDefaults.CookieOrTokenScheme"/> policy scheme allows the
/// test-connection endpoint to authenticate via the ambient session cookie, unlike bearer/OpenIddict-token
/// requests. Requests that authenticated that way must also prove themselves with a valid
/// antiforgery token, since they can be replayed cross-site by a browser that still holds the
/// cookie. Bearer-token requests carry no antiforgery cookie and are left untouched.
/// </summary>
internal sealed class RequireAntiforgeryForCookieAuthFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var identity = context.HttpContext.User.Identity;

        if (identity is not { IsAuthenticated: true }
            || !string.Equals(identity.AuthenticationType, IdentityConstants.ApplicationScheme, StringComparison.Ordinal))
        {
            return await next(context);
        }

        var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();

        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest();
        }

        return await next(context);
    }
}

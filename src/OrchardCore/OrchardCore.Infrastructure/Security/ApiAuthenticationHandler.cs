using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Security;

/// <summary>
/// Provides a delegating logic for API authentication.
/// If a specific scheme handler (e.g., Bearer or OpenIddict) is registered, it is tried first.
/// If no token-based scheme succeeds, falls back to the default authentication scheme
/// (typically cookies), allowing same-origin requests from the admin UI to authenticate.
/// If no scheme succeeds, returns an anonymous user.
/// </summary>
public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthorizationOptions>
{
    private readonly IOptions<AuthenticationOptions> _authenticationOptions;

    public ApiAuthenticationHandler(
        IOptions<AuthenticationOptions> authenticationOptions,
        IOptionsMonitor<ApiAuthorizationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _authenticationOptions = authenticationOptions;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // If the request includes an explicit Authorization header, only token-based
        // authentication should be used — never fall back to cookies silently.
        var hasExplicitCredentials = !string.IsNullOrEmpty(Context.Request.Headers.Authorization);

        // Try the configured API authentication scheme first (Bearer / OpenIddict).
        if (_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
        {
            var result = await Context.AuthenticateAsync(Options.ApiAuthenticationScheme);

            if (result.Succeeded || hasExplicitCredentials)
            {
                return result;
            }
        }
        else if (hasExplicitCredentials)
        {
            return AuthenticateResult.NoResult();
        }

        // No credentials were provided. Fall back to the default scheme (cookies)
        // so that same-origin requests from admin UI components
        // (e.g., Vue.js widgets, Swagger "Try it out") work.
        var defaultScheme = _authenticationOptions.Value.DefaultAuthenticateScheme;

        if (!string.IsNullOrEmpty(defaultScheme)
            && defaultScheme != Options.ApiAuthenticationScheme
            && _authenticationOptions.Value.SchemeMap.ContainsKey(defaultScheme))
        {
            var cookieResult = await Context.AuthenticateAsync(defaultScheme);

            if (cookieResult.Succeeded)
            {
                return cookieResult;
            }
        }

        return AuthenticateResult.NoResult();
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (!_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
        {
            Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
            return Task.CompletedTask;
        }

        var statusCodePagesFeature = Context.Features.Get<IStatusCodePagesFeature>();
        if (statusCodePagesFeature != null)
        {
            statusCodePagesFeature.Enabled = false;
        }

        return Context.ChallengeAsync(Options.ApiAuthenticationScheme);
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        if (!_authenticationOptions.Value.SchemeMap.ContainsKey(Options.ApiAuthenticationScheme))
        {
            Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
            return Task.CompletedTask;
        }

        var statusCodePagesFeature = Context.Features.Get<IStatusCodePagesFeature>();
        if (statusCodePagesFeature != null)
        {
            statusCodePagesFeature.Enabled = false;
        }

        return Context.ForbidAsync(Options.ApiAuthenticationScheme);
    }
}

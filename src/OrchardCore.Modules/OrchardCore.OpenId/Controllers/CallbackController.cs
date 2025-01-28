using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using OrchardCore.Modules;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.AspNetCore.OpenIddictClientAspNetCoreConstants;

namespace OrchardCore.OpenId.Controllers;

[AllowAnonymous, Feature(OpenIdConstants.Features.Client)]
public class CallbackController : Controller
{
    private readonly OpenIddictClientService _service;

    public CallbackController(OpenIddictClientService service)
        => _service = service;

    [IgnoreAntiforgeryToken]
    public async Task<ActionResult> LogInCallback()
    {
        var response = HttpContext.GetOpenIddictClientResponse();
        if (response != null)
        {
            return View("Error", new ErrorViewModel
            {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }

        var request = HttpContext.GetOpenIddictClientRequest();
        if (request == null)
        {
            return NotFound();
        }

        // Retrieve the authorization data validated by OpenIddict as part of the callback handling.
        var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

        // Important: if the remote server doesn't support OpenID Connect and doesn't expose a userinfo endpoint,
        // result.Principal.Identity will represent an unauthenticated identity and won't contain any claim.
        //
        // Such identities cannot be used as-is to build an authentication cookie in ASP.NET Core, as the
        // antiforgery stack requires at least a name claim to bind CSRF cookies to the user's identity.
        if (result.Principal is not ClaimsPrincipal { Identity.IsAuthenticated: true })
        {
            throw new InvalidOperationException("The external authorization data cannot be used for authentication.");
        }

        // Build an identity based on the external claims and that will be used to create the authentication cookie.
        //
        // Note: for compatibility reasons, the claims are mapped to their WS-Federation equivalent
        // using the default mapping provided by JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.
        var claims = result.Principal.Claims.Select(claim =>
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.TryGetValue(claim.Type, out var type) ?
            new Claim(type, claim.Value, claim.ValueType, claim.Issuer, claim.OriginalIssuer, claim.Subject) : claim);

        var identity = new ClaimsIdentity(claims,
            authenticationType: CookieAuthenticationDefaults.AuthenticationScheme,
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);

        // Build the authentication properties based on the properties that were added when the challenge was triggered.
        var properties = new AuthenticationProperties(result.Properties.Items)
        {
            RedirectUri = result.Properties.RedirectUri ?? "/"
        };

        // If enabled, preserve the received tokens in the authentication cookie.
        //
        // Note: for compatibility reasons, the tokens are stored using the same
        // names as the Microsoft ASP.NET Core OIDC client: when both a frontchannel
        // and a backchannel token exist, the backchannel one is always preferred.
        var registration = await _service.GetClientRegistrationByIdAsync(result.Principal.FindFirstValue(Claims.Private.RegistrationId));
        if (registration.Properties.TryGetValue(nameof(OpenIdClientSettings), out var settings) &&
            settings is OpenIdClientSettings { StoreExternalTokens: true })
        {
            List<AuthenticationToken> tokens = [];

            if (!string.IsNullOrEmpty(result.Properties.GetTokenValue(Tokens.BackchannelAccessToken)) ||
                !string.IsNullOrEmpty(result.Properties.GetTokenValue(Tokens.FrontchannelAccessToken)))
            {
                tokens.Add(new AuthenticationToken
                {
                    Name = Parameters.AccessToken,
                    Value = result.Properties.GetTokenValue(Tokens.BackchannelAccessToken) ??
                            result.Properties.GetTokenValue(Tokens.FrontchannelAccessToken)
                });
            }

            if (!string.IsNullOrEmpty(result.Properties.GetTokenValue(Tokens.BackchannelAccessTokenExpirationDate)) ||
                !string.IsNullOrEmpty(result.Properties.GetTokenValue(Tokens.FrontchannelAccessTokenExpirationDate)))
            {
                tokens.Add(new AuthenticationToken
                {
                    Name = "expires_at",
                    Value = result.Properties.GetTokenValue(Tokens.BackchannelAccessTokenExpirationDate) ??
                            result.Properties.GetTokenValue(Tokens.FrontchannelAccessTokenExpirationDate)
                });
            }

            if (!string.IsNullOrEmpty(result.Properties.GetTokenValue(Tokens.BackchannelIdentityToken)) ||
                !string.IsNullOrEmpty(result.Properties.GetTokenValue(Tokens.FrontchannelIdentityToken)))
            {
                tokens.Add(new AuthenticationToken
                {
                    Name = Parameters.IdToken,
                    Value = result.Properties.GetTokenValue(Tokens.BackchannelIdentityToken) ??
                            result.Properties.GetTokenValue(Tokens.FrontchannelIdentityToken)
                });
            }

            if (!string.IsNullOrEmpty(result.Properties.GetTokenValue(Tokens.RefreshToken)))
            {
                tokens.Add(new AuthenticationToken
                {
                    Name = Parameters.RefreshToken,
                    Value = result.Properties.GetTokenValue(Tokens.RefreshToken)
                });
            }

            properties.StoreTokens(tokens);
        }

        else
        {
            properties.StoreTokens(Enumerable.Empty<AuthenticationToken>());
        }

        // Ask the cookie authentication handler to return a new cookie and redirect
        // the user agent to the return URL stored in the authentication properties.
        return SignIn(new ClaimsPrincipal(identity), properties);
    }

    [IgnoreAntiforgeryToken]
    public async Task<ActionResult> LogOutCallback()
    {
        var response = HttpContext.GetOpenIddictClientResponse();
        if (response != null)
        {
            return View("Error", new ErrorViewModel
            {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }

        var request = HttpContext.GetOpenIddictClientRequest();
        if (request == null)
        {
            return NotFound();
        }

        // Retrieve the data stored by OpenIddict in the state token created when the logout was triggered
        // and redirect the user agent to the URI attached to the authentication properties, if applicable.
        var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
        if (!string.IsNullOrEmpty(result.Properties.RedirectUri))
        {
            return Redirect(result.Properties.RedirectUri);
        }

        // Otherwise, return the user agent to the static URI attached to the client registration if it it managed by Orchard.
        var registration = await _service.GetClientRegistrationByIdAsync(result.Principal.FindFirstValue(Claims.Private.RegistrationId));
        if (registration.Properties.TryGetValue(nameof(OpenIdClientSettings), out var settings) &&
            settings is OpenIdClientSettings { SignedOutRedirectUri: { Length: > 0 } uri })
        {
            return Redirect(uri);
        }

        // As a last resort, return the user agent to the home page.
        return Redirect("/");
    }
}

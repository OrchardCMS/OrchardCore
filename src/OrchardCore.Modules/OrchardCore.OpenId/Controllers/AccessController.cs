using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Routing;
using OrchardCore.Security.Services;
using OrchardCore.Users.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId.Controllers;

// Note: the error descriptions used in this controller are deliberately not localized as
// the OAuth 2.0 specification only allows select US-ASCII characters in error_description.
[Authorize, Feature(OpenIdConstants.Features.Server)]
public sealed class AccessController : Controller
{
    private readonly IOpenIdApplicationManager _applicationManager;
    private readonly IOpenIdAuthorizationManager _authorizationManager;
    private readonly IOpenIdScopeManager _scopeManager;
    private readonly ShellSettings _shellSettings;

    public AccessController(
        IOpenIdApplicationManager applicationManager,
        IOpenIdAuthorizationManager authorizationManager,
        IOpenIdScopeManager scopeManager,
        ShellSettings shellSettings)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _shellSettings = shellSettings;
    }

    [AllowAnonymous, DisableCors, HttpGet, HttpPost, IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var response = HttpContext.GetOpenIddictServerResponse();
        if (response != null)
        {
            return View("Error", new ErrorViewModel
            {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }

        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return NotFound();
        }

        // Retrieve the claims stored in the authentication cookie.
        // If they can't be extracted, redirect the user to the login page.
        var result = await HttpContext.AuthenticateAsync();
        if (result == null || !result.Succeeded || request.HasPrompt(Prompts.Login))
        {
            return RedirectToLoginPage(request);
        }

        // If a max_age parameter was provided, ensure that the cookie is not too old.
        // If it's too old, automatically redirect the user agent to the login page.
        if (request.MaxAge != null && result.Properties.IssuedUtc != null &&
            DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value))
        {
            return RedirectToLoginPage(request);
        }

        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
            throw new InvalidOperationException("The application details cannot be found.");

        var authorizations = await _authorizationManager.FindAsync(
            subject: result.Principal.GetUserIdentifier(),
            client: await _applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        switch (await _applicationManager.GetConsentTypeAsync(application))
        {
            case ConsentTypes.External when authorizations.Count == 0:
                return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The logged in user is not allowed to access this client application."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            case ConsentTypes.Implicit:
            case ConsentTypes.External when authorizations.Count > 0:
            case ConsentTypes.Explicit when authorizations.Count > 0 && !request.HasPrompt(Prompts.Consent):
                var identity = new ClaimsIdentity(result.Principal.Claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(OpenIdConstants.Claims.EntityType, OpenIdConstants.EntityTypes.User));

                PopulateIdentityClaims(result.Principal, identity);

                identity.SetScopes(request.GetScopes());
                identity.SetResources(await GetResourcesAsync(request.GetScopes()));

                // Automatically create a permanent authorization to avoid requiring explicit consent
                // for future authorization or token requests containing the same scopes.
                var authorization = authorizations.LastOrDefault();
                authorization ??= await _authorizationManager.CreateAsync(
                    identity: identity,
                    subject: identity.GetUserIdentifier(),
                    client: await _applicationManager.GetIdAsync(application),
                    type: AuthorizationTypes.Permanent,
                    scopes: identity.GetScopes());

                identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            case ConsentTypes.Explicit when request.HasPrompt(Prompts.None):
                return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "Interactive user consent is required."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            default:
                return View(new AuthorizeViewModel
                {
                    ApplicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application),
                    RequestId = request.RequestId,
                    Scope = request.Scope
                });
        }

        IActionResult RedirectToLoginPage(OpenIddictRequest request)
        {
            // If the client application requested promptless authentication,
            // return an error indicating that the user is not logged in.
            if (request.HasPrompt(Prompts.None))
            {
                return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            string GetRedirectUrl()
            {
                // Override the prompt parameter to prevent infinite authentication/authorization loops.
                var parameters = Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                parameters[Parameters.Prompt] = "continue";

                return Request.PathBase + Request.Path + QueryString.Create(parameters);
            }

            return Challenge(new AuthenticationProperties
            {
                RedirectUri = GetRedirectUrl()
            });
        }
    }

    private static void PopulateIdentityClaims(ClaimsPrincipal principal, ClaimsIdentity identity)
    {
        // Note: while ASP.NET Core Identity uses the legacy WS-Federation claims (exposed by the ClaimTypes class),
        // OpenIddict uses the newer JWT claims defined by the OpenID Connect specification. To ensure the mandatory
        // subject claim is correctly populated (and avoid an InvalidOperationException), it's manually added here.
        if (!principal.HasClaim(static claim => claim.Type is Claims.Subject))
        {
            identity.AddClaim(new Claim(Claims.Subject, principal.GetUserIdentifier()));
        }

        if (!principal.HasClaim(static claim => claim.Type is Claims.Name))
        {
            identity.AddClaim(new Claim(Claims.Name, principal.GetUserName()));
        }

        if (!principal.HasClaim(static claim => claim.Type is Claims.Role))
        {
            foreach (var role in principal.GetRoles())
            {
                identity.AddClaim(new Claim(Claims.Role, role));
            }
        }
    }

    [ActionName(nameof(Authorize)), DisableCors]
    [FormValueRequired("submit.Accept"), HttpPost]
    public async Task<IActionResult> AuthorizeAccept()
    {
        // Warning: unlike the main Authorize method, this method MUST NOT be decorated with
        // [IgnoreAntiforgeryToken] as we must be able to reject authorization requests
        // sent by a malicious client that could abuse this interactive endpoint to silently
        // get codes/tokens without the user explicitly approving the authorization demand.

        var response = HttpContext.GetOpenIddictServerResponse();
        if (response != null)
        {
            return View("Error", new ErrorViewModel
            {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }

        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return NotFound();
        }

        var application = await _applicationManager.FindByClientIdAsync(request.ClientId)
            ?? throw new InvalidOperationException("The application details cannot be found.");

        var authorizations = await _authorizationManager.FindAsync(
            subject: User.GetUserIdentifier(),
            client: await _applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        // Note: the same check is already made in the GET action but is repeated
        // here to ensure a malicious user can't abuse this POST endpoint and
        // force it to return a valid response without the external authorization.
        switch (await _applicationManager.GetConsentTypeAsync(application))
        {
            case ConsentTypes.External when authorizations.Count == 0:
                return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The logged in user is not allowed to access this client application."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            default:
                var identity = new ClaimsIdentity(User.Claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(OpenIdConstants.Claims.EntityType, OpenIdConstants.EntityTypes.User));

                PopulateIdentityClaims(User, identity);

                identity.SetScopes(request.GetScopes());
                identity.SetResources(await GetResourcesAsync(request.GetScopes()));

                // Automatically create a permanent authorization to avoid requiring explicit consent
                // for future authorization or token requests containing the same scopes.
                var authorization = authorizations.LastOrDefault();
                authorization ??= await _authorizationManager.CreateAsync(
                    identity: identity,
                    subject: identity.GetUserIdentifier(),
                    client: await _applicationManager.GetIdAsync(application),
                    type: AuthorizationTypes.Permanent,
                    scopes: identity.GetScopes());

                identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }

    [ActionName(nameof(Authorize)), DisableCors]
    [FormValueRequired("submit.Deny"), HttpPost]
    public IActionResult AuthorizeDeny()
    {
        var response = HttpContext.GetOpenIddictServerResponse();
        if (response != null)
        {
            return View("Error", new ErrorViewModel
            {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }

        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return NotFound();
        }

        return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [AllowAnonymous, DisableCors, HttpGet, HttpPost, IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        var response = HttpContext.GetOpenIddictServerResponse();
        if (response != null)
        {
            return View("Error", new ErrorViewModel
            {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }

        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(request.PostLogoutRedirectUri))
        {
            // If the user is not logged in, allow redirecting the user agent back to the
            // specified post_logout_redirect_uri without rendering a confirmation form.
            var result = await HttpContext.AuthenticateAsync();
            if (result == null || !result.Succeeded)
            {
                return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
        }

        return View(new LogoutViewModel
        {
            RequestId = request.RequestId
        });
    }

    [ActionName(nameof(Logout)), AllowAnonymous, DisableCors]
    [FormValueRequired("submit.Accept"), HttpPost]
    public async Task<IActionResult> LogoutAccept()
    {
        var response = HttpContext.GetOpenIddictServerResponse();
        if (response != null)
        {
            return View("Error", new ErrorViewModel
            {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }

        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return NotFound();
        }

        // Warning: unlike the main Logout method, this method MUST NOT be decorated with
        // [IgnoreAntiforgeryToken] as we must be able to reject end session requests
        // sent by a malicious client that could abuse this interactive endpoint to silently
        // log the user out without the user explicitly approving the log out operation.

        await HttpContext.SignOutAsync();

        // If no post_logout_redirect_uri was specified, redirect the user agent
        // to the root page, that should correspond to the home page in most cases.
        if (string.IsNullOrEmpty(request.PostLogoutRedirectUri))
        {
            return Redirect("~/");
        }

        return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [ActionName(nameof(Logout)), AllowAnonymous, DisableCors]
    [FormValueRequired("submit.Deny"), HttpPost]
    public IActionResult LogoutDeny()
    {
        var response = HttpContext.GetOpenIddictServerResponse();
        if (response != null)
        {
            return View("Error", new ErrorViewModel
            {
                Error = response.Error,
                ErrorDescription = response.ErrorDescription
            });
        }

        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return NotFound();
        }

        return Redirect("~/");
    }

    [AllowAnonymous, HttpPost]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public Task<IActionResult> Token()
    {
        // Warning: this action is decorated with IgnoreAntiforgeryTokenAttribute to override
        // the global antiforgery token validation policy applied by the MVC modules stack,
        // which is required for this stateless OAuth2/OIDC token endpoint to work correctly.
        // To prevent effective CSRF/session fixation attacks, this action MUST NOT return
        // an authentication cookie or try to establish an ASP.NET Core user session.

        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return Task.FromResult((IActionResult)NotFound());
        }

        if (request.IsPasswordGrantType())
        {
            return ExchangePasswordGrantType(request);
        }

        if (request.IsClientCredentialsGrantType())
        {
            return ExchangeClientCredentialsGrantType(request);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            return ExchangeAuthorizationCodeOrRefreshTokenGrantType(request);
        }

        throw new NotSupportedException("The specified grant type is not supported.");
    }

    private async Task<IActionResult> ExchangeClientCredentialsGrantType(OpenIddictRequest request)
    {
        if (request.HasScope(Scopes.OfflineAccess))
        {
            return Forbid(new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidScope,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    "The 'offline_access' scope is not allowed when using the client credentials grant."
            }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Note: client authentication is always enforced by OpenIddict before this action is invoked.
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
            throw new InvalidOperationException("The application details cannot be found.");

        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            Claims.Name, Claims.Role);

        identity.AddClaim(new Claim(OpenIdConstants.Claims.EntityType, OpenIdConstants.EntityTypes.Application));
        identity.AddClaim(new Claim(Claims.Subject, request.ClientId));

        // Always add a "name" claim for grant_type=client_credentials in both
        // access and identity tokens even if the "name" scope wasn't requested.
        identity.AddClaim(new Claim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application))
            .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));

        // If the role service is available, add all the role claims
        // associated with the application roles in the database.
        var roleService = HttpContext.RequestServices.GetService<IRoleService>();

        foreach (var role in await _applicationManager.GetRolesAsync(application))
        {
            // Since the claims added in this block have a dynamic name, directly set the destinations
            // here instead of relying on the GetDestination() helper that only works with static claims.

            identity.AddClaim(new Claim(identity.RoleClaimType, role)
                .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));

            if (roleService != null)
            {
                foreach (var claim in await roleService.GetRoleClaimsAsync(role))
                {
                    identity.AddClaim(claim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
                }
            }
        }

        identity.SetScopes(request.GetScopes());
        identity.SetResources(await GetResourcesAsync(request.GetScopes()));
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangePasswordGrantType(OpenIddictRequest request)
    {
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
            throw new InvalidOperationException("The application details cannot be found.");

        // By design, the password flow requires direct username/password validation, which is performed by
        // the user service. If this service is not registered, prevent the password flow from being used.
        var service = HttpContext.RequestServices.GetService<IUserService>();
        if (service == null)
        {
            return Forbid(new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.UnsupportedGrantType,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                    "The resource owner password credentials grant is not supported."
            }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        string error = null;
        var user = await service.AuthenticateAsync(request.Username, request.Password, (key, message) => error = message);
        if (user == null)
        {
            return Forbid(new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = error
            }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var principal = await service.CreatePrincipalAsync(user);

        var authorizations = await _authorizationManager.FindAsync(
            subject: principal.GetUserIdentifier(),
            client: await _applicationManager.GetIdAsync(application),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        // If the application is configured to use external consent,
        // reject the request if no existing authorization can be found.
        switch (await _applicationManager.GetConsentTypeAsync(application))
        {
            case ConsentTypes.External when authorizations.Count == 0:
                return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The logged in user is not allowed to access this client application."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var identity = (ClaimsIdentity)principal.Identity;
        identity.AddClaim(new Claim(OpenIdConstants.Claims.EntityType, OpenIdConstants.EntityTypes.User));

        PopulateIdentityClaims(principal, identity);

        identity.SetScopes(request.GetScopes());
        identity.SetResources(await GetResourcesAsync(request.GetScopes()));

        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var authorization = authorizations.FirstOrDefault();
        authorization ??= await _authorizationManager.CreateAsync(
            identity: identity,
            subject: identity.GetUserIdentifier(),
            client: await _applicationManager.GetIdAsync(application),
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(GetDestinations);

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangeAuthorizationCodeOrRefreshTokenGrantType(OpenIddictRequest request)
    {
        // Retrieve the claims principal stored in the authorization code/refresh token.
        var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme) ??
            throw new InvalidOperationException("The user principal cannot be resolved.");

        if (request.IsRefreshTokenGrantType())
        {
            var type = info.Principal.FindFirst(OpenIdConstants.Claims.EntityType)?.Value;
            if (!string.Equals(type, OpenIdConstants.EntityTypes.User, StringComparison.Ordinal))
            {
                return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.UnauthorizedClient,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The refresh token grant type is not allowed for refresh tokens retrieved using the client credentials flow."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
        }

        // By default, re-use the principal stored in the authorization code/refresh token.
        var principal = info.Principal;

        // If the user service is available, try to refresh the principal by retrieving
        // the user object from the database and creating a new claims-based principal.
        var service = HttpContext.RequestServices.GetService<IUserService>();
        if (service != null)
        {
            var user = await service.GetUserByUniqueIdAsync(principal.GetUserIdentifier());
            if (user != null)
            {
                principal = await service.CreatePrincipalAsync(user);
                // Copy the granted scopes and resources from the original authorization code/refresh token principal
                principal.SetScopes(info.Principal.GetScopes());
                principal.SetResources(await GetResourcesAsync(info.Principal.GetScopes()));
            }
        }

        var identity = (ClaimsIdentity)principal.Identity;
        identity.AddClaim(new Claim(OpenIdConstants.Claims.EntityType, OpenIdConstants.EntityTypes.User));

        PopulateIdentityClaims(principal, identity);

        identity.SetDestinations(GetDestinations);

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            // If the claim already includes destinations (set before this helper is called), flow them as-is.
            case string when claim.GetDestinations() is { IsDefaultOrEmpty: false } destinations:
                return destinations;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp":
                return [];

            // Only add the claim to the id_token if the corresponding scope was granted.
            // The other claims will only be added to the access_token.
            case OpenIdConstants.Claims.EntityType:
            case Claims.Name when claim.Subject.HasScope(Scopes.Profile):
            case Claims.Email when claim.Subject.HasScope(Scopes.Email):
            case Claims.Role when claim.Subject.HasScope(Scopes.Roles):
                return new[]
                {
                    Destinations.AccessToken,
                    Destinations.IdentityToken
                };

            default: return new[] { Destinations.AccessToken };
        }
    }

    private async Task<IEnumerable<string>> GetResourcesAsync(ImmutableArray<string> scopes)
    {
        // Note: the current tenant name is always added as a valid resource/audience,
        // which allows the end user to use the corresponding tokens with the APIs
        // located in the current tenant without having to explicitly register a scope.
        var resources = new List<string>()
        {
            OpenIdConstants.Prefixes.Tenant + _shellSettings.Name
        };

        await foreach (var resource in _scopeManager.ListResourcesAsync(scopes))
        {
            resources.Add(resource);
        }

        return resources;
    }
}

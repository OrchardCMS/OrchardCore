using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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

namespace OrchardCore.OpenId.Controllers
{
    // Note: the error descriptions used in this controller are deliberately not localized as
    // the OAuth 2.0 specification only allows select US-ASCII characters in error_description.
    [Authorize, Feature(OpenIdConstants.Features.Server)]
    public class AccessController : Controller
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

        [AllowAnonymous, HttpGet, HttpPost, IgnoreAntiforgeryToken]
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
                case ConsentTypes.External when !authorizations.Any():
                    return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizations.Any():
                case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
                    var identity = new ClaimsIdentity(result.Principal.Claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    identity.AddClaim(OpenIdConstants.Claims.EntityType, OpenIdConstants.EntityTypes.User,
                        Destinations.AccessToken, Destinations.IdentityToken);

                    // Note: while ASP.NET Core Identity uses the legacy WS-Federation claims (exposed by the ClaimTypes class),
                    // OpenIddict uses the newer JWT claims defined by the OpenID Connect specification. To ensure the mandatory
                    // subject claim is correctly populated (and avoid an InvalidOperationException), it's manually added here.
                    if (string.IsNullOrEmpty(result.Principal.FindFirst(Claims.Subject)?.Value))
                    {
                        identity.AddClaim(new Claim(Claims.Subject, result.Principal.GetUserIdentifier()));
                    }
                    if (string.IsNullOrEmpty(result.Principal.FindFirst(Claims.Name)?.Value))
                    {
                        identity.AddClaim(new Claim(Claims.Name, result.Principal.GetUserName()));
                    }

                    principal.SetScopes(request.GetScopes());
                    principal.SetResources(await GetResourcesAsync(request.GetScopes()));

                    // Automatically create a permanent authorization to avoid requiring explicit consent
                    // for future authorization or token requests containing the same scopes.
                    var authorization = authorizations.LastOrDefault();
                    if (authorization == null)
                    {
                        authorization = await _authorizationManager.CreateAsync(
                            principal: principal,
                            subject: principal.GetUserIdentifier(),
                            client: await _applicationManager.GetIdAsync(application),
                            type: AuthorizationTypes.Permanent,
                            scopes: principal.GetScopes());
                    }

                    principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

                    foreach (var claim in principal.Claims)
                    {
                        claim.SetDestinations(GetDestinations(claim, principal));
                    }

                    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

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

        [ActionName(nameof(Authorize))]
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
                case ConsentTypes.External when !authorizations.Any():
                    return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                default:
                    var identity = new ClaimsIdentity(User.Claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    identity.AddClaim(OpenIdConstants.Claims.EntityType, OpenIdConstants.EntityTypes.User,
                        Destinations.AccessToken, Destinations.IdentityToken);

                    // Note: while ASP.NET Core Identity uses the legacy WS-Federation claims (exposed by the ClaimTypes class),
                    // OpenIddict uses the newer JWT claims defined by the OpenID Connect specification. To ensure the mandatory
                    // subject claim is correctly populated (and avoid an InvalidOperationException), it's manually added here.
                    if (string.IsNullOrEmpty(User.FindFirst(Claims.Subject)?.Value))
                    {
                        identity.AddClaim(new Claim(Claims.Subject, User.GetUserIdentifier()));
                    }
                    if (string.IsNullOrEmpty(User.FindFirst(Claims.Name)?.Value))
                    {
                        identity.AddClaim(new Claim(Claims.Name, User.GetUserName()));
                    }

                    principal.SetScopes(request.GetScopes());
                    principal.SetResources(await GetResourcesAsync(request.GetScopes()));

                    // Automatically create a permanent authorization to avoid requiring explicit consent
                    // for future authorization or token requests containing the same scopes.
                    var authorization = authorizations.LastOrDefault();
                    if (authorization == null)
                    {
                        authorization = await _authorizationManager.CreateAsync(
                            principal: principal,
                            subject: principal.GetUserIdentifier(),
                            client: await _applicationManager.GetIdAsync(application),
                            type: AuthorizationTypes.Permanent,
                            scopes: principal.GetScopes());
                    }

                    principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

                    foreach (var claim in principal.Claims)
                    {
                        claim.SetDestinations(GetDestinations(claim, principal));
                    }

                    return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
        }

        [ActionName(nameof(Authorize))]
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

        [AllowAnonymous, HttpGet, HttpPost, IgnoreAntiforgeryToken]
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

        [ActionName(nameof(Logout)), AllowAnonymous]
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

        [ActionName(nameof(Logout)), AllowAnonymous]
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
            // Note: client authentication is always enforced by OpenIddict before this action is invoked.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("The application details cannot be found.");

            var identity = new ClaimsIdentity(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                Claims.Name, Claims.Role);

            identity.AddClaim(OpenIdConstants.Claims.EntityType, OpenIdConstants.EntityTypes.Application,
                Destinations.AccessToken, Destinations.IdentityToken);

            identity.AddClaim(Claims.Subject, request.ClientId,
                Destinations.AccessToken, Destinations.IdentityToken);

            identity.AddClaim(Claims.Name,
                await _applicationManager.GetDisplayNameAsync(application),
                Destinations.AccessToken, Destinations.IdentityToken);

            // If the role service is available, add all the role claims
            // associated with the application roles in the database.
            var roleService = HttpContext.RequestServices.GetService<IRoleService>();

            foreach (var role in await _applicationManager.GetRolesAsync(application))
            {
                identity.AddClaim(identity.RoleClaimType, role,
                    Destinations.AccessToken, Destinations.IdentityToken);

                if (roleService != null)
                {
                    foreach (var claim in await roleService.GetRoleClaimsAsync(role))
                    {
                        identity.AddClaim(claim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
                    }
                }
            }

            var principal = new ClaimsPrincipal(identity);
            principal.SetResources(await GetResourcesAsync(request.GetScopes()));

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
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
                case ConsentTypes.External when !authorizations.Any():
                    return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var identity = (ClaimsIdentity)principal.Identity;

            identity.AddClaim(OpenIdConstants.Claims.EntityType, OpenIdConstants.EntityTypes.User,
                Destinations.AccessToken, Destinations.IdentityToken);

            // Note: while ASP.NET Core Identity uses the legacy WS-Federation claims (exposed by the ClaimTypes class),
            // OpenIddict uses the newer JWT claims defined by the OpenID Connect specification. To ensure the mandatory
            // subject claim is correctly populated (and avoid an InvalidOperationException), it's manually added here.
            if (string.IsNullOrEmpty(principal.FindFirst(Claims.Subject)?.Value))
            {
                identity.AddClaim(new Claim(Claims.Subject, principal.GetUserIdentifier()));
            }
            if (string.IsNullOrEmpty(principal.FindFirst(Claims.Name)?.Value))
            {
                identity.AddClaim(new Claim(Claims.Name, principal.GetUserName()));
            }

            principal.SetScopes(request.GetScopes());
            principal.SetResources(await GetResourcesAsync(request.GetScopes()));

            // Automatically create a permanent authorization to avoid requiring explicit consent
            // for future authorization or token requests containing the same scopes.
            var authorization = authorizations.FirstOrDefault();
            if (authorization == null)
            {
                authorization = await _authorizationManager.CreateAsync(
                    principal: principal,
                    subject: principal.GetUserIdentifier(),
                    client: await _applicationManager.GetIdAsync(application),
                    type: AuthorizationTypes.Permanent,
                    scopes: principal.GetScopes());
            }

            principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

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
                if (!string.Equals(type, OpenIdConstants.EntityTypes.User))
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
                }
            }

            var identity = (ClaimsIdentity)principal.Identity;

            identity.AddClaim(OpenIdConstants.Claims.EntityType, OpenIdConstants.EntityTypes.User,
                Destinations.AccessToken, Destinations.IdentityToken);

            // Note: while ASP.NET Core Identity uses the legacy WS-Federation claims (exposed by the ClaimTypes class),
            // OpenIddict uses the newer JWT claims defined by the OpenID Connect specification. To ensure the mandatory
            // subject claim is correctly populated (and avoid an InvalidOperationException), it's manually added here.
            if (string.IsNullOrEmpty(principal.FindFirst(Claims.Subject)?.Value))
            {
                identity.AddClaim(new Claim(Claims.Subject, principal.GetUserIdentifier()));
            }
            if (string.IsNullOrEmpty(principal.FindFirst(Claims.Name)?.Value))
            {
                identity.AddClaim(new Claim(Claims.Name, principal.GetUserName()));
            }

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal));
            }

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            switch (claim.Type)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp":
                    break;

                // Only add the claim to the id_token if the corresponding scope was granted.
                // The other claims will only be added to the access_token.
                case OpenIdConstants.Claims.EntityType:
                case Claims.Name when principal.HasScope(Scopes.Profile):
                case Claims.Email when principal.HasScope(Scopes.Email):
                case Claims.Role when principal.HasScope(Scopes.Roles):
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;
                    break;

                default:
                    yield return Destinations.AccessToken;
                    break;
            }
        }

        private async Task<IEnumerable<string>> GetResourcesAsync(ImmutableArray<string> scopes)
        {
            // Note: the current tenant name is always added as a valid resource/audience,
            // which allows the end user to use the corresponding tokens with the APIs
            // located in the current tenant without having to explicitly register a scope.
            var resources = new List<string>(1)
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
}

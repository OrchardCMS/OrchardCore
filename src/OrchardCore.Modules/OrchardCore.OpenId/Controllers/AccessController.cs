using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.ActionConstraints;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Filters;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Security;
using OrchardCore.Users;

namespace OrchardCore.OpenId.Controllers
{
    [Authorize, Feature(OpenIdConstants.Features.Server), OpenIdController]
    public class AccessController : Controller
    {
        private readonly IOpenIdApplicationManager _applicationManager;
        private readonly IOpenIdAuthorizationManager _authorizationManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IOpenIdScopeManager _scopeManager;
        private readonly ShellSettings _shellSettings;
        private readonly SignInManager<IUser> _signInManager;
        private readonly RoleManager<IRole> _roleManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IStringLocalizer<AccessController> T;

        public AccessController(
            IOpenIdApplicationManager applicationManager,
            IOpenIdAuthorizationManager authorizationManager,
            IOptions<IdentityOptions> identityOptions,
            IStringLocalizer<AccessController> localizer,
            IOpenIdScopeManager scopeManager,
            ShellSettings shellSettings,
            IOpenIdServerService serverService,
            RoleManager<IRole> roleManager,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager)
        {
            T = localizer;
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
            _shellSettings = shellSettings;
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [AllowAnonymous, HttpGet, HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize(OpenIdConnectRequest request)
        {
            // Retrieve the claims stored in the authentication cookie.
            // If they can't be extracted, redirect the user to the login page.
            var result = await HttpContext.AuthenticateAsync();
            if (result == null || !result.Succeeded || request.HasPrompt(OpenIddictConstants.Prompts.Login))
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

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIddictConstants.Errors.InvalidClient,
                    ErrorDescription = T["The specified 'client_id' parameter is invalid."]
                });
            }

            var authorizations = await _authorizationManager.FindAsync(
                subject: _userManager.GetUserId(result.Principal),
                client : await _applicationManager.GetIdAsync(application),
                status : OpenIddictConstants.Statuses.Valid,
                type   : OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes : ImmutableArray.CreateRange(request.GetScopes()));

            switch (await _applicationManager.GetConsentTypeAsync(application))
            {
                case OpenIddictConstants.ConsentTypes.External when authorizations.IsEmpty:
                    return RedirectToClient(new OpenIdConnectResponse
                    {
                        Error = OpenIddictConstants.Errors.ConsentRequired,
                        ErrorDescription = T["The logged in user is not allowed to access this client application."]
                    });

                case OpenIddictConstants.ConsentTypes.Implicit:
                case OpenIddictConstants.ConsentTypes.External when authorizations.Any():
                case OpenIddictConstants.ConsentTypes.Explicit when authorizations.Any() &&
                    !request.HasPrompt(OpenIddictConstants.Prompts.Consent):
                    return await IssueTokensAsync(result.Principal, request, application, authorizations.LastOrDefault());

                case OpenIddictConstants.ConsentTypes.Explicit when request.HasPrompt(OpenIddictConstants.Prompts.None):
                    return RedirectToClient(new OpenIdConnectResponse
                    {
                        Error = OpenIddictConstants.Errors.ConsentRequired,
                        ErrorDescription = T["Interactive user consent is required."]
                    });

                default:
                    return View(new AuthorizeViewModel
                    {
                        ApplicationName = await _applicationManager.GetDisplayNameAsync(application),
                        RequestId = request.RequestId,
                        Scope = request.Scope
                    });
            }
        }

        [ActionName(nameof(Authorize))]
        [FormValueRequired("submit.Accept"), HttpPost]
        public async Task<IActionResult> AuthorizeAccept(OpenIdConnectRequest request)
        {
            // Warning: unlike the main Authorize method, this method MUST NOT be decorated with
            // [IgnoreAntiforgeryToken] as we must be able to reject authorization requests
            // sent by a malicious client that could abuse this interactive endpoint to silently
            // get codes/tokens without the user explicitly approving the authorization demand.

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIddictConstants.Errors.InvalidClient,
                    ErrorDescription = T["The specified 'client_id' parameter is invalid."]
                });
            }

            var authorizations = await _authorizationManager.FindAsync(
                subject: _userManager.GetUserId(User),
                client : await _applicationManager.GetIdAsync(application),
                status : OpenIddictConstants.Statuses.Valid,
                type   : OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes : ImmutableArray.CreateRange(request.GetScopes()));

            // Note: the same check is already made in the GET action but is repeated
            // here to ensure a malicious user can't abuse this POST endpoint and
            // force it to return a valid response without the external authorization.
            switch (await _applicationManager.GetConsentTypeAsync(application))
            {
                case OpenIddictConstants.ConsentTypes.External when authorizations.IsEmpty:
                    return RedirectToClient(new OpenIdConnectResponse
                    {
                        Error = OpenIddictConstants.Errors.ConsentRequired,
                        ErrorDescription = T["The logged in user is not allowed to access this client application."]
                    });

                default:
                    return await IssueTokensAsync(User, request, application, authorizations.LastOrDefault());
            }
        }

        [ActionName(nameof(Authorize))]
        [FormValueRequired("submit.Deny"), HttpPost]
        public IActionResult AuthorizeDeny() => Forbid(OpenIddictServerDefaults.AuthenticationScheme);

        [AllowAnonymous, HttpGet, HttpPost, IgnoreAntiforgeryToken]
        public async Task<IActionResult> Logout(OpenIdConnectRequest request)
        {
            if (!string.IsNullOrEmpty(request.PostLogoutRedirectUri))
            {
                // If the user is not logged in, allow redirecting the user agent back to the
                // specified post_logout_redirect_uri without rendering a confirmation form.
                var result = await HttpContext.AuthenticateAsync();
                if (result == null || !result.Succeeded)
                {
                    return SignOut(OpenIddictServerDefaults.AuthenticationScheme);
                }
            }

            return View(new LogoutViewModel
            {
                RequestId = request.RequestId
            });
        }

        [ActionName(nameof(Logout)), AllowAnonymous]
        [FormValueRequired("submit.Accept"), HttpPost]
        public async Task<IActionResult> LogoutAccept(OpenIdConnectRequest request)
        {
            // Warning: unlike the main Logout method, this method MUST NOT be decorated with
            // [IgnoreAntiforgeryToken] as we must be able to reject end session requests
            // sent by a malicious client that could abuse this interactive endpoint to silently
            // log the user out without the user explicitly approving the log out operation.

            await _signInManager.SignOutAsync();

            // If no post_logout_redirect_uri was specified, redirect the user agent
            // to the root page, that should correspond to the home page in most cases.
            if (string.IsNullOrEmpty(request.PostLogoutRedirectUri))
            {
                return Redirect("~/");
            }

            return SignOut(OpenIddictServerDefaults.AuthenticationScheme);
        }

        [ActionName(nameof(Logout)), AllowAnonymous]
        [FormValueRequired("submit.Deny"), HttpPost]
        public IActionResult LogoutDeny() => Redirect("~/");

        [AllowAnonymous, HttpPost]
        [IgnoreAntiforgeryToken]
        [Produces("application/json")]
        public async Task<IActionResult> Token(OpenIdConnectRequest request)
        {
            // Warning: this action is decorated with IgnoreAntiforgeryTokenAttribute to override
            // the global antiforgery token validation policy applied by the MVC modules stack,
            // which is required for this stateless OAuth2/OIDC token endpoint to work correctly.
            // To prevent effective CSRF/session fixation attacks, this action MUST NOT return
            // an authentication cookie or try to establish an ASP.NET Core user session.

            if (request.IsPasswordGrantType())
            {
                return await ExchangePasswordGrantType(request);
            }

            if (request.IsClientCredentialsGrantType())
            {
                return await ExchangeClientCredentialsGrantType(request);
            }

            if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                return await ExchangeAuthorizationCodeOrRefreshTokenGrantType(request);
            }

            throw new NotSupportedException("The specified grant type is not supported.");
        }

        private async Task<IActionResult> ExchangeClientCredentialsGrantType(OpenIdConnectRequest request)
        {
            // Note: client authentication is always enforced by OpenIddict before this action is invoked.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidClient,
                    ErrorDescription = T["The specified 'client_id' parameter is invalid."]
                });
            }

            var identity = new ClaimsIdentity(
                OpenIddictServerDefaults.AuthenticationScheme,
                OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);

            identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId);
            identity.AddClaim(OpenIddictConstants.Claims.Name,
                await _applicationManager.GetDisplayNameAsync(application),
                OpenIddictConstants.Destinations.AccessToken,
                OpenIddictConstants.Destinations.IdentityToken);

            foreach (var role in await _applicationManager.GetRolesAsync(application))
            {
                identity.AddClaim(identity.RoleClaimType, role,
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken);

                foreach (var claim in await _roleManager.GetClaimsAsync(await _roleManager.FindByIdAsync(role)))
                {
                    identity.AddClaim(claim.Type, claim.Value,
                        OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken);
                }
            }

            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIddictServerDefaults.AuthenticationScheme);

            ticket.SetResources(await GetResourcesAsync(request.GetScopes()));

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> ExchangePasswordGrantType(OpenIdConnectRequest request)
        {
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIddictConstants.Errors.InvalidClient,
                    ErrorDescription = T["The specified 'client_id' parameter is invalid."]
                });
            }

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The username/password couple is invalid."]
                });
            }

            var authorizations = await _authorizationManager.FindAsync(
                subject: await _userManager.GetUserIdAsync(user),
                client : await _applicationManager.GetIdAsync(application),
                status : OpenIddictConstants.Statuses.Valid,
                type   : OpenIddictConstants.AuthorizationTypes.Permanent,
                scopes : ImmutableArray.CreateRange(request.GetScopes()));

            // If the application is configured to use external consent,
            // reject the request if no existing authorization can be found.
            switch (await _applicationManager.GetConsentTypeAsync(application))
            {
                case OpenIddictConstants.ConsentTypes.External when authorizations.IsEmpty:
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIddictConstants.Errors.ConsentRequired,
                        ErrorDescription = T["The logged in user is not allowed to access this client application."]
                    });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.IsNotAllowed)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The specified user is not allowed to sign in."]
                });
            }
            else if (result.RequiresTwoFactor)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The specified user is not allowed to sign in using the password method."]
                });
            }
            else if (!result.Succeeded)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The username/password couple is invalid."]
                });
            }

            var ticket = await CreateTicketAsync(user, application, authorizations.LastOrDefault(), request);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> ExchangeAuthorizationCodeOrRefreshTokenGrantType(OpenIdConnectRequest request)
        {
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIddictConstants.Errors.InvalidClient,
                    ErrorDescription = T["The specified 'client_id' parameter is invalid."]
                });
            }

            // Retrieve the claims principal stored in the authorization code/refresh token.
            var info = await HttpContext.AuthenticateAsync(OpenIddictServerDefaults.AuthenticationScheme);
            Debug.Assert(info.Principal != null, "The user principal shouldn't be null.");

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            var user = await _userManager.GetUserAsync(info.Principal);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The token is no longer valid."]
                });
            }

            // Ensure the user is still allowed to sign in.
            if (!await _signInManager.CanSignInAsync(user))
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The user is no longer allowed to sign in."]
                });
            }

            // Create a new authentication ticket, but reuse the properties stored in the
            // authorization code/refresh token, including the scopes originally granted.
            var ticket = await CreateTicketAsync(user, application, null, request, info.Properties);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> IssueTokensAsync(
            ClaimsPrincipal principal, OpenIdConnectRequest request,
            object application, object authorization = null)
        {
            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIddictConstants.Errors.ServerError,
                    ErrorDescription = T["An internal error has occurred."]
                });
            }

            var ticket = await CreateTicketAsync(user, application, authorization, request);

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(
            IUser user, object application, object authorization,
            OpenIdConnectRequest request, AuthenticationProperties properties = null)
        {
            Debug.Assert(request.IsAuthorizationRequest() || request.IsTokenRequest(),
                "The request should be an authorization or token request.");

            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var identity = (ClaimsIdentity) principal.Identity;

            // Note: while ASP.NET Core Identity uses the legacy WS-Federation claims (exposed by the ClaimTypes class),
            // OpenIddict uses the newer JWT claims defined by the OpenID Connect specification. To ensure the mandatory
            // subject claim is correctly populated (and avoid an InvalidOperationException), it's manually added here.
            if (string.IsNullOrEmpty(principal.FindFirstValue(OpenIddictConstants.Claims.Subject)))
            {
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user)));
            }

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal, properties,
                OpenIddictServerDefaults.AuthenticationScheme);

            if (request.IsAuthorizationRequest() || (!request.IsAuthorizationCodeGrantType() &&
                                                     !request.IsRefreshTokenGrantType()))
            {
                // Set the list of scopes granted to the client application.
                // Note: the offline_access scope must be granted
                // to allow OpenIddict to return a refresh token.
                ticket.SetScopes(request.GetScopes());
                ticket.SetResources(await GetResourcesAsync(request.GetScopes()));

                // If the request is an authorization request, automatically create
                // a permanent authorization to avoid requiring explicit consent for
                // future authorization or token requests containing the same scopes.
                if (authorization == null && request.IsAuthorizationRequest())
                {
                    authorization = await _authorizationManager.CreateAsync(
                        principal : ticket.Principal,
                        subject   : await _userManager.GetUserIdAsync(user),
                        client    : await _applicationManager.GetIdAsync(application),
                        type      : OpenIddictConstants.AuthorizationTypes.Permanent,
                        scopes    : ImmutableArray.CreateRange(ticket.GetScopes()),
                        properties: ImmutableDictionary.CreateRange(ticket.Properties.Items));
                }

                if (authorization != null)
                {
                    // Attach the authorization identifier to the authentication ticket.
                    ticket.SetProperty(OpenIddictConstants.Properties.InternalAuthorizationId,
                        await _authorizationManager.GetIdAsync(authorization));
                }
            }

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                {
                    continue;
                }

                var destinations = new List<string>
                {
                    OpenIddictConstants.Destinations.AccessToken
                };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIddictConstants.Claims.Name && ticket.HasScope(OpenIddictConstants.Scopes.Profile)) ||
                    (claim.Type == OpenIddictConstants.Claims.Email && ticket.HasScope(OpenIddictConstants.Scopes.Email)) ||
                    (claim.Type == OpenIddictConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIddictConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            return ticket;
        }

        private async Task<IEnumerable<string>> GetResourcesAsync(IEnumerable<string> scopes)
        {
            // Note: the current tenant name is always added as a valid resource/audience,
            // which allows the end user to use the corresponding tokens with the APIs
            // located in the current tenant without having to explicitly register a scope.
            var resources = new List<string>(1);
            resources.Add(OpenIdConstants.Prefixes.Tenant + _shellSettings.Name);
            resources.AddRange(await _scopeManager.ListResourcesAsync(scopes.ToImmutableArray()));

            return resources;
        }

        private IActionResult RedirectToClient(OpenIdConnectResponse response)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string>
            {
                [OpenIddictConstants.Properties.Error] = response.Error,
                [OpenIddictConstants.Properties.ErrorDescription] = response.ErrorDescription,
                [OpenIddictConstants.Properties.ErrorUri] = response.ErrorUri,
            });

            return Forbid(properties, OpenIddictServerDefaults.AuthenticationScheme);
        }

        private IActionResult RedirectToLoginPage(OpenIdConnectRequest request)
        {
            // If the client application requested promptless authentication,
            // return an error indicating that the user is not logged in.
            if (request.HasPrompt(OpenIddictConstants.Prompts.None))
            {
                return RedirectToClient(new OpenIdConnectResponse
                {
                    Error = OpenIddictConstants.Errors.LoginRequired,
                    ErrorDescription = T["The user is not logged in."]
                });
            }

            string GetRedirectUrl()
            {
                // Override the prompt parameter to prevent infinite authentication/authorization loops.
                var parameters = Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                parameters[OpenIddictConstants.Parameters.Prompt] = "continue";

                return Request.PathBase + Request.Path + QueryString.Create(parameters);
            }

            var properties = new AuthenticationProperties
            {
                RedirectUri = GetRedirectUrl()
            };

            return Challenge(properties, IdentityConstants.ApplicationScheme);
        }
    }
}

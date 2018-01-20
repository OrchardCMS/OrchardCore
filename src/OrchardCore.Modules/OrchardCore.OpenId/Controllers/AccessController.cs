using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Core;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Services.Managers;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Security;
using OrchardCore.Users;

namespace OrchardCore.OpenId.Controllers
{
    [Authorize]
    public class AccessController : Controller
    {
        private readonly OpenIdApplicationManager _applicationManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly SignInManager<IUser> _signInManager;
        private readonly IOpenIdService _openIdService;
        private readonly RoleManager<IRole> _roleManager;
        private readonly UserManager<IUser> _userManager;
        private readonly IStringLocalizer<AccessController> T;

        public AccessController(
            OpenIdApplicationManager applicationManager,
            IOptions<IdentityOptions> identityOptions,
            IStringLocalizer<AccessController> localizer,
            IOpenIdService openIdService,
            RoleManager<IRole> roleManager,
            SignInManager<IUser> signInManager,
            UserManager<IUser> userManager)
        {
            T = localizer;
            _applicationManager = applicationManager;
            _identityOptions = identityOptions;
            _openIdService = openIdService;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> Authorize()
        {
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response != null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = response.Error,
                    ErrorDescription = response.ErrorDescription
                });
            }

            // If the request is missing, this likely means that
            // this endpoint was not enabled in the settings.
            // In this case, simply return a 404 response.
            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return NotFound();
            }

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = T["Details concerning the calling client application cannot be found in the database"]
                });
            }

            if (request.HasScope(OpenIdConnectConstants.Scopes.OfflineAccess) &&
                !await _applicationManager.IsRefreshTokenFlowAllowedAsync(application, HttpContext.RequestAborted))
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = T["Offline scope is not allowed for this OpenID Connect Application"]
                });
            }

            if (request.IsAuthorizationCodeFlow() &&
                !await _applicationManager.IsAuthorizationCodeFlowAllowedAsync(application, HttpContext.RequestAborted))
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.UnauthorizedClient,
                    ErrorDescription = T["Authorization Code Flow is not allowed for this OpenID Connect Application"]
                });
            }

            if (request.IsImplicitFlow() &&
                !await _applicationManager.IsImplicitFlowAllowedAsync(application, HttpContext.RequestAborted))
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.UnauthorizedClient,
                    ErrorDescription = T["Implicit Flow is not allowed for this OpenID Connect Application"]
                });
            }

            if (request.IsHybridFlow() &&
                !await _applicationManager.IsHybridFlowAllowedAsync(application, HttpContext.RequestAborted))
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.UnauthorizedClient,
                    ErrorDescription = T["Hybrid Flow is not allowed for this OpenID Connect Application"]
                });
            }

            if (Request.HasFormContentType)
            {
                if (!string.IsNullOrEmpty(Request.Form["submit.Accept"]))
                {
                    return await IssueAccessIdentityTokensAsync(request);
                }
                else if (!string.IsNullOrEmpty(Request.Form["submit.Deny"]))
                {
                    return Forbid(OpenIdConnectServerDefaults.AuthenticationScheme);
                }
            }

            if (await _applicationManager.IsConsentRequiredAsync(application, HttpContext.RequestAborted))
            {
                return await IssueAccessIdentityTokensAsync(request);
            }

            return View(new AuthorizeViewModel
            {
                ApplicationName = await _applicationManager.GetDisplayNameAsync(application, HttpContext.RequestAborted),
                RequestId = request.RequestId,
                Scope = request.Scope
            });
        }

        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> Logout()
        {
            var response = HttpContext.GetOpenIdConnectResponse();
            if (response != null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = response.Error,
                    ErrorDescription = response.ErrorDescription
                });
            }

            // If the request is missing, this likely means that
            // this endpoint was not enabled in the settings.
            // In this case, simply return a 404 response.
            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return NotFound();
            }

            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application.
            return SignOut(OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        [AllowAnonymous, HttpPost]
        [IgnoreAntiforgeryToken]
        [Produces("application/json")]
        public async Task<IActionResult> Token()
        {
            // Warning: this action is decorated with IgnoreAntiforgeryTokenAttribute to override
            // the global antiforgery token validation policy applied by the MVC modules stack,
            // which is required for this stateless OAuth2/OIDC token endpoint to work correctly.
            // To prevent effective CSRF/session fixation attacks, this action MUST NOT return
            // an authentication cookie or try to establish an ASP.NET Core user session.

            // If the request is missing, this likely means that
            // this endpoint was not enabled in the settings.
            // In this case, simply return a 404 response.
            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return NotFound();
            }

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);
            if (application == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = T["Details concerning the calling client application cannot be found in the database"]
                });
            }

            if (request.HasScope(OpenIdConnectConstants.Scopes.OfflineAccess) &&
                !await _applicationManager.IsRefreshTokenFlowAllowedAsync(application, HttpContext.RequestAborted))
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidRequest,
                    ErrorDescription = T["Offline scope is not allowed for this OpenID Connect Application"]
                });
            }

            if (request.IsPasswordGrantType())
            {
                if (!await _applicationManager.IsPasswordFlowAllowedAsync(application, HttpContext.RequestAborted))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.UnauthorizedClient,
                        ErrorDescription = T["Password Flow is not allowed for this OpenID Connect Application"]
                    });
                }

                return await ExchangePasswordGrantType(request);
            }

            if (request.IsClientCredentialsGrantType())
            {
                if (!await _applicationManager.IsClientCredentialsFlowAllowedAsync(application, HttpContext.RequestAborted))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.UnauthorizedClient,
                        ErrorDescription = T["Client Credentials Flow is not allowed for this OpenID Connect Application"]
                    });
                }

                return await ExchangeClientCredentialsGrantType(request);
            }

            if (request.IsAuthorizationCodeGrantType())
            {
                if (!await _applicationManager.IsAuthorizationCodeFlowAllowedAsync(application, HttpContext.RequestAborted))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.UnauthorizedClient,
                        ErrorDescription = T["Authorization Code Flow is not allowed for this OpenID Connect Application"]
                    });
                }

                return await ExchangeAuthorizationCodeOrRefreshTokenGrantType(request);
            }

            if (request.IsRefreshTokenGrantType())
            {
                if (!await _applicationManager.IsRefreshTokenFlowAllowedAsync(application, HttpContext.RequestAborted))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.UnauthorizedClient,
                        ErrorDescription = T["Refresh Token Flow is not allowed for this OpenID Connect Application"]
                    });
                }

                return await ExchangeAuthorizationCodeOrRefreshTokenGrantType(request);
            }

            throw new NotSupportedException("The specified grant type is not supported.");
        }

        private async Task<IActionResult> ExchangeClientCredentialsGrantType(OpenIdConnectRequest request)
        {
            // Note: client authentication is always enforced by OpenIddict before this action is invoked.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);
            if (application == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = T["The client application is unknown."]
                });
            }

            var identity = new ClaimsIdentity(
                OpenIdConnectServerDefaults.AuthenticationScheme,
                OpenIdConnectConstants.Claims.Name,
                OpenIdConnectConstants.Claims.Role);

            identity.AddClaim(OpenIdConnectConstants.Claims.Subject, request.ClientId);
            identity.AddClaim(OpenIdConnectConstants.Claims.Name,
                await _applicationManager.GetDisplayNameAsync(application, HttpContext.RequestAborted),
                OpenIdConnectConstants.Destinations.AccessToken,
                OpenIdConnectConstants.Destinations.IdentityToken);

            foreach (var role in await _applicationManager.GetRolesAsync(application, HttpContext.RequestAborted))
            {
                identity.AddClaim(identity.RoleClaimType, role,
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);

                foreach (var claim in await _roleManager.GetClaimsAsync(await _roleManager.FindByIdAsync(role)))
                {
                    identity.AddClaim(claim.Type, claim.Value, OpenIdConnectConstants.Destinations.AccessToken,
                                                               OpenIdConnectConstants.Destinations.IdentityToken);
                }
            }

            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);

            ticket.SetResources(await GetResourcesAsync(request));

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> ExchangePasswordGrantType(OpenIdConnectRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The username/password couple is invalid."]
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.IsNotAllowed)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The specified user is not allowed to sign in."]
                });
            }
            else if (result.RequiresTwoFactor)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The specified user is not allowed to sign in using the password method."]
                });
            }
            else if (!result.Succeeded)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The username/password couple is invalid."]
                });
            }

            var ticket = await CreateTicketAsync(request, user);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> ExchangeAuthorizationCodeOrRefreshTokenGrantType(OpenIdConnectRequest request)
        {
            // Retrieve the claims principal stored in the authorization code/refresh token.
            //var info = await HttpContext.Authentication.GetAuthenticateInfoAsync(
            var info = await HttpContext.AuthenticateAsync(
                OpenIdConnectServerDefaults.AuthenticationScheme);

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            // Note: if you want to automatically invalidate the authorization code/refresh token
            // when the user password/roles change, use the following line instead:
            // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
            var user = await _userManager.GetUserAsync(info.Principal);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The token is no longer valid."]
                });
            }

            // Ensure the user is still allowed to sign in.
            if (!await _signInManager.CanSignInAsync(user))
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The user is no longer allowed to sign in."]
                });
            }

            // Create a new authentication ticket, but reuse the properties stored in the
            // authorization code/refresh token, including the scopes originally granted.
            var ticket = await CreateTicketAsync(request, user, info.Properties);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> IssueAccessIdentityTokensAsync(OpenIdConnectRequest request)
        {
            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = T["An internal error has occurred"]
                });
            }

            var ticket = await CreateTicketAsync(request, user);

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(
            OpenIdConnectRequest request, IUser user,
            AuthenticationProperties properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var identity = (ClaimsIdentity) principal.Identity;

            // Note: while ASP.NET Core Identity uses the legacy WS-Federation claims (exposed by the ClaimTypes class),
            // OpenIddict uses the newer JWT claims defined by the OpenID Connect specification. To ensure the mandatory
            // subject claim is correctly populated (and avoid an InvalidOperationException), it's manually added here.
            if (string.IsNullOrEmpty(principal.FindFirstValue(OpenIdConnectConstants.Claims.Subject)))
            {
                identity.AddClaim(new Claim(OpenIdConnectConstants.Claims.Subject, await _userManager.GetUserIdAsync(user)));
            }

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal, properties,
                OpenIdConnectServerDefaults.AuthenticationScheme);

            if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            {
                // Set the list of scopes granted to the client application.
                // Note: the offline_access scope must be granted
                // to allow OpenIddict to return a refresh token.
                ticket.SetScopes(new[]
                {
                    OpenIdConnectConstants.Scopes.OpenId,
                    OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIdConnectConstants.Scopes.OfflineAccess,
                    OpenIddictConstants.Scopes.Roles
                }.Intersect(request.GetScopes()));

                ticket.SetResources(await GetResourcesAsync(request));
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
                    OpenIdConnectConstants.Destinations.AccessToken
                };

                // Only add the iterated claim to the id_token if the corresponding scope was granted to the client application.
                // The other claims will only be added to the access_token, which is encrypted when using the default format.
                if ((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles)))
                {
                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            return ticket;
        }

        private async Task<IEnumerable<string>> GetResourcesAsync(OpenIdConnectRequest request)
        {
            var settings = await _openIdService.GetOpenIdSettingsAsync();

            // If at least one resource was specified, use Intersect() to exclude values that are not
            // listed as valid audiences in the OpenID Connect settings associated with the tenant.
            var resources = request.GetResources();
            if (resources.Any())
            {
                return resources.Intersect(settings.Audiences);
            }

            return settings.Audiences;
        }
    }
}

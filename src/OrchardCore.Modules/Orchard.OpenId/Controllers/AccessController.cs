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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Orchard.Mvc.ActionConstraints;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Core;
using Orchard.OpenId.Models;
using Orchard.OpenId.ViewModels;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.OpenId.Controllers
{
    [Authorize]
    public class AccessController : Controller
    {
        private readonly OpenIddictApplicationManager<OpenIdApplication> _applicationManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IRoleClaimStore<Role> _roleStore;
        private readonly IStringLocalizer<AccessController> T;

        public AccessController(
            OpenIddictApplicationManager<OpenIdApplication> applicationManager,
            IOptions<IdentityOptions> identityOptions,
            IStringLocalizer<AccessController> localizer,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IRoleClaimStore<Role> roleStore)
        {
            T = localizer;
            _applicationManager = applicationManager;
            _identityOptions = identityOptions;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleStore = roleStore;
        }

        [HttpGet]
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

            if (request.HasScope(OpenIdConnectConstants.Scopes.OfflineAccess) && !application.AllowRefreshTokenFlow)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = T["Offline scope is not allowed for this OpenID Connect Application"]
                });
            }

            if (request.IsAuthorizationCodeFlow() && !application.AllowAuthorizationCodeFlow)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.UnauthorizedClient,
                    ErrorDescription = T["Authorization Code Flow is not allowed for this OpenID Connect Application"]
                });
            }

            if (request.IsImplicitFlow() && !application.AllowImplicitFlow)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.UnauthorizedClient,
                    ErrorDescription = T["Implicit Flow is not allowed for this OpenID Connect Application"]
                });
            }

            if (request.IsHybridFlow() && !application.AllowHybridFlow)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.UnauthorizedClient,
                    ErrorDescription = T["Hybrid Flow is not allowed for this OpenID Connect Application"]
                });
            }

            if (application.SkipConsent)
            {
                return await IssueAccessIdentityTokensAsync(request);
            }

            return View(new AuthorizeViewModel
            {
                ApplicationName = application.DisplayName,
                RequestId = request.RequestId,
                Scope = request.Scope
            });
        }

        [ActionName(nameof(Authorize))]
        [HttpPost, FormValueRequired("submit.Accept")]
        public async Task<IActionResult> Accept()
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

            return await IssueAccessIdentityTokensAsync(request);
        }

        [ActionName(nameof(Authorize))]
        [HttpPost, FormValueRequired("submit.Deny")]
        public IActionResult Deny()
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

            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = T["The authorization server is not correctly configured."]
                });
            }

            // Notify OpenIddict that the authorization grant has been denied by the resource owner
            // to redirect the user agent to the client application using the appropriate response_mode.
            return Forbid(OpenIdConnectServerDefaults.AuthenticationScheme);
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

            if (request.HasScope(OpenIdConnectConstants.Scopes.OfflineAccess) && !application.AllowRefreshTokenFlow)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidRequest,
                    ErrorDescription = T["Offline scope is not allowed for this OpenID Connect Application"]
                });
            }

            if (request.IsPasswordGrantType())
            {
                if (!application.AllowPasswordFlow)
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
                if (!application.AllowClientCredentialsFlow)
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
                if (!application.AllowAuthorizationCodeFlow)
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
                if (!application.AllowRefreshTokenFlow)
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

            identity.AddClaim(OpenIdConnectConstants.Claims.Subject, application.ClientId);
            identity.AddClaim(OpenIdConnectConstants.Claims.Name,
                await _applicationManager.GetDisplayNameAsync(application, HttpContext.RequestAborted),
                OpenIdConnectConstants.Destinations.AccessToken,
                OpenIdConnectConstants.Destinations.IdentityToken);

            foreach (var roleName in application.RoleNames)
            {
                identity.AddClaim(identity.RoleClaimType, roleName,
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);

                foreach (var claim in await _roleStore.GetClaimsAsync(await _roleStore.FindByIdAsync(roleName, HttpContext.RequestAborted)))
                {
                    identity.AddClaim(claim.Type, claim.Value, OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken);
                }
            }

            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);

            ticket.SetResources(request.GetResources());

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

            // Ensure the user is allowed to sign in.
            if (!await _signInManager.CanSignInAsync(user))
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The specified user is not allowed to sign in."]
                });
            }

            // Reject the token request if two-factor authentication has been enabled by the user.
            if (_userManager.SupportsUserTwoFactor && await _userManager.GetTwoFactorEnabledAsync(user))
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The specified user is not allowed to sign in."]
                });
            }

            // Ensure the user is not already locked out.
            if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The username/password couple is invalid."]
                });
            }

            // Ensure the password is valid.
            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                if (_userManager.SupportsUserLockout)
                {
                    await _userManager.AccessFailedAsync(user);
                }

                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The username/password couple is invalid."]
                });
            }

            if (_userManager.SupportsUserLockout)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            var ticket = await CreateTicketAsync(request, user);

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        private async Task<IActionResult> ExchangeAuthorizationCodeOrRefreshTokenGrantType(OpenIdConnectRequest request)
        {
            // Retrieve the claims principal stored in the authorization code/refresh token.
            var info = await HttpContext.Authentication.GetAuthenticateInfoAsync(
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
            OpenIdConnectRequest request, User user,
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

                ticket.SetResources(request.GetResources());
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
    }
}

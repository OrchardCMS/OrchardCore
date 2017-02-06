using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OpenIddict;
using Orchard.Mvc;
using Orchard.OpenId.Services;
using Orchard.OpenId.ViewModels;
using Orchard.Users.Models;

namespace Orchard.OpenId.Controllers
{
    [Authorize]
    public class AccessController : Controller
    {
        private readonly IOpenIdApplicationManager _applicationManager;
        private readonly SignInManager<User> _signInManager;
        private readonly OpenIddictUserManager<User> _userManager;
        private readonly IStringLocalizer<AccessController> T;
        private readonly IOpenIdService _openIdService;
        private readonly IOpenIdApplicationManager _openIdApplicationManager;

        public AccessController(
            IOpenIdApplicationManager applicationManager,
            SignInManager<User> signInManager,
            OpenIddictUserManager<User> userManager,
            IStringLocalizer<AccessController> stringLocalizer,
            IOpenIdService openIdService,
            IOpenIdApplicationManager openIdApplicationManager)
        {
            T = stringLocalizer;
            _applicationManager = applicationManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _openIdService = openIdService;
            _openIdApplicationManager = openIdApplicationManager;
        }

        [AllowAnonymous, HttpPost]
        [IgnoreAntiforgeryToken]
        [Produces("application/json")]
        public async Task<IActionResult> Token()
        {
            // Warning: this action is decorated with IgnoreAntiforgeryTokenAttribute to override
            // the global antiforgery token validation policy applied by Orchard.Hosting.Web,
            // which is required for this stateless OAuth2/OIDC token endpoint to work correctly.
            // To prevent effective CSRF/session fixation attacks, this action MUST NOT
            // return an authentication cookie or try to establish an ASP.NET Core session.
            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = T["The authorization server is not correctly configured."]
                });
            }

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
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

            if (request.IsPasswordGrantType())
            {
                if (!application.AllowPasswordFlow)
                {
                    return View("Error", new ErrorViewModel
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidClient,
                        ErrorDescription = T["Password Flow is not allowed for this OpenID Connect Application"]
                    });
                }
                return await ExchangePasswordGrantType(request);
            }

            if (request.IsClientCredentialsGrantType())
            {
                if (!application.AllowClientCredentialsFlow)
                {
                    return View("Error", new ErrorViewModel
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidClient,
                        ErrorDescription = T["Client Credentials Flow is not allowed for this OpenID Connect Application"]
                    });
                }
                return await ExchangeClientCredentialsGrantType(request);
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedResponseType,
                ErrorDescription = T["The specified grant type is not supported."]
            });
        }

        private async Task<IActionResult> ExchangeClientCredentialsGrantType(OpenIdConnectRequest request)
        {
            // Note: client authentication is always enforced by OpenIddict before this action is invoked.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = T["The client application is unknown."]
                });
            }

            var identity = await _applicationManager.CreateIdentityAsync(application, request.GetScopes());

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);

            ticket.SetResources(request.GetResources());
            ticket.SetScopes(request.GetScopes());

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
                    ErrorDescription = "The specified user is not allowed to sign in."
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

            var identity = await _userManager.CreateIdentityAsync(user, request.GetScopes());

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);

            ticket.SetResources(request.GetResources());
            ticket.SetScopes(request.GetScopes());

            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
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

            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = T["The authorization server is not correctly configured."]
                });
            }

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
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
            
            if (request.IsAuthorizationCodeFlow() && !application.AllowPasswordFlow)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = T["Password Flow is not allowed for this OpenID Connect Application"]
                });
            }

            if (request.IsImplicitFlow() && !application.AllowImplicitFlow)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = T["Implicit Flow is not allowed for this OpenID Connect Application"]
                });
            }

            var openIdSettings = await _openIdService.GetOpenIdSettingsAsync();
            // Note: It is needed to check openIdSettings.AllowHybridFlow because OpenIddict doesn't have an specific configuration for AllowHybridFlow.             
            if (request.IsHybridFlow() && (!openIdSettings.AllowHybridFlow || !application.AllowHybridFlow))
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = T["Hybrid Flow is not allowed for this OpenID Connect Application"]
                });
            }

            if (application.SkipConsent)
            {
                return await IssueAccessIdentityTokens(request);
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

            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = T["The authorization server is not correctly configured."]
                });
            }

            return await IssueAccessIdentityTokens(request);
        }

        private async Task<IActionResult> IssueAccessIdentityTokens(OpenIdConnectRequest request)
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

            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var identity = await _userManager.CreateIdentityAsync(user, request.GetScopes());

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIdConnectServerDefaults.AuthenticationScheme);

            ticket.SetResources(request.GetResources());
            ticket.SetScopes(request.GetScopes());

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
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

            var request = HttpContext.GetOpenIdConnectRequest();
            if (request == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = T["The authorization server is not correctly configured."]
                });
            }

            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application.
            return SignOut(OpenIdConnectServerDefaults.AuthenticationScheme);
        }
    }
}

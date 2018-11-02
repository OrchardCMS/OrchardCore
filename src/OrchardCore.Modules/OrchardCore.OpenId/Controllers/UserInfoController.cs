using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OpenIddict.Abstractions;
using OrchardCore.Modules;
using OrchardCore.OpenId.Filters;
using OrchardCore.Users;

namespace OrchardCore.OpenId.Controllers
{
    [Feature(OpenIdConstants.Features.Server)]
    [OpenIdController, SkipStatusCodePages]
    public class UserInfoController : Controller
    {
        private readonly IStringLocalizer<UserInfoController> T;
        private readonly UserManager<IUser> _userManager;

        public UserInfoController(
            IStringLocalizer<UserInfoController> localizer,
            UserManager<IUser> userManager)
        {
            T = localizer;
            _userManager = userManager;
        }

        // GET/POST: /connect/userinfo
        [AcceptVerbs("GET", "POST")]
        [IgnoreAntiforgeryToken]
        [Produces("application/json")]
        public async Task<IActionResult> Me()
        {
            // Warning: this action is decorated with IgnoreAntiforgeryTokenAttribute to override
            // the global antiforgery token validation policy applied by the MVC modules stack,
            // which is required for this stateless OpenID userinfo endpoint to work correctly.
            // To prevent effective CSRF/session fixation attacks, this action MUST NOT return
            // an authentication cookie or try to establish an ASP.NET Core user session.

            // Note: this controller doesn't use [Authorize] to prevent MVC Core from throwing
            // an exception if the JWT/validation handler was not registered (e.g because the
            // OpenID server feature was not enabled or because the configuration was invalid).
            var result = await HttpContext.AuthenticateAsync(OpenIdConstants.Schemes.Userinfo);
            if (result?.Principal == null)
            {
                return Challenge(OpenIdConstants.Schemes.Userinfo);
            }

            var user = await _userManager.GetUserAsync(result.Principal);
            if (user == null)
            {
                return Challenge(OpenIdConstants.Schemes.Userinfo);
            }

            var claims = new JObject();

            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            claims[OpenIdConnectConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user);

            if (_userManager.SupportsUserEmail &&
                result.Principal.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Email))
            {
                claims[OpenIdConnectConstants.Claims.Email] = await _userManager.GetEmailAsync(user);
                claims[OpenIdConnectConstants.Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
            }

            if (_userManager.SupportsUserPhoneNumber &&
                result.Principal.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Phone))
            {
                claims[OpenIdConnectConstants.Claims.PhoneNumber] = await _userManager.GetPhoneNumberAsync(user);
                claims[OpenIdConnectConstants.Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
            }

            if (_userManager.SupportsUserRole &&
                result.Principal.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIddictConstants.Scopes.Roles))
            {
                claims[OpenIddictConstants.Claims.Roles] = JArray.FromObject(await _userManager.GetRolesAsync(user));
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Ok(claims);
        }
    }
}

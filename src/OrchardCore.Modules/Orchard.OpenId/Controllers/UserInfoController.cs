using System.Threading.Tasks;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OpenIddict.Core;
using Orchard.Users.Models;

namespace Orchard.OpenId.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = OAuthValidationDefaults.AuthenticationScheme)]
    [EnableCors(Constants.OpenIdConnectPolicy)]
    public class UserInfoController : Controller
    {
        private readonly IStringLocalizer<AccessController> T;
        private readonly UserManager<User> _userManager;

        public UserInfoController(
            IStringLocalizer<AccessController> localizer,
            UserManager<User> userManager)
        {
            T = localizer;
            _userManager = userManager;
        }

        // GET: /Orchard.OpenId/UserInfo/Me
        [HttpGet]
        [IgnoreAntiforgeryToken]
        [Produces("application/json")]
        public async Task<IActionResult> Me()
        {
            // Warning: this action is decorated with IgnoreAntiforgeryTokenAttribute to override
            // the global antiforgery token validation policy applied by the MVC modules stack,
            // which is required for this stateless OpenID userinfo endpoint to work correctly.
            // To prevent effective CSRF/session fixation attacks, this action MUST NOT return
            // an authentication cookie or try to establish an ASP.NET Core user session.

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = T["The user profile is no longer available."]
                });
            }

            var claims = new JObject();

            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            claims[OpenIdConnectConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user);

            if (_userManager.SupportsUserEmail &&
                User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Email))
            {
                claims[OpenIdConnectConstants.Claims.Email] = await _userManager.GetEmailAsync(user);
                claims[OpenIdConnectConstants.Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
            }

            if (_userManager.SupportsUserPhoneNumber &&
                User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Phone))
            {
                claims[OpenIdConnectConstants.Claims.PhoneNumber] = await _userManager.GetPhoneNumberAsync(user);
                claims[OpenIdConnectConstants.Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
            }

            if (_userManager.SupportsUserRole &&
                User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIddictConstants.Scopes.Roles))
            {
                claims[OpenIddictConstants.Claims.Roles] = JArray.FromObject(await _userManager.GetRolesAsync(user));
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Json(claims);
        }
    }
}

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OrchardCore.Modules;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId.Controllers
{
    // Note: the error descriptions used in this controller are deliberately not localized as
    // the OAuth 2.0 specification only allows select US-ASCII characters in error_description.
    [Feature(OpenIdConstants.Features.Server), SkipStatusCodePages]
    public class UserInfoController : Controller
    {
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

            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
            {
                return NotFound();
            }

            // Note: this controller doesn't use [Authorize] to prevent MVC from throwing
            // an exception if the OpenIddict server handler was not registered (e.g because the
            // OpenID server feature was not enabled or because the configuration was invalid).
            var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))?.Principal;
            if (principal == null)
            {
                return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Ensure the access token represents a user and not an application.
            var type = principal.FindFirst(OpenIdConstants.Claims.EntityType)?.Value;
            if (!string.Equals(type, OpenIdConstants.EntityTypes.User))
            {
                return Forbid(new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidRequest,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The userinfo endpoint can only be used with access tokens representing users."
                }), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var claims = new Dictionary<string, object>();

            if (principal.HasScope(Scopes.Profile))
            {
                var preferredUsername = principal.FindFirst(Claims.PreferredUsername)?.Value;
                if (!string.IsNullOrEmpty(preferredUsername))
                {
                    claims[Claims.PreferredUsername] = preferredUsername;
                }

                var name = principal.FindFirst(Claims.Name)?.Value ?? principal.FindFirst(ClaimTypes.Name)?.Value;
                if (!string.IsNullOrEmpty(name))
                {
                    claims[Claims.Name] = name;
                }

                var familyName = principal.FindFirst(Claims.FamilyName)?.Value ?? principal.FindFirst(ClaimTypes.Surname)?.Value;
                if (!string.IsNullOrEmpty(familyName))
                {
                    claims[Claims.FamilyName] = familyName;
                }

                var givenName = principal.FindFirst(Claims.GivenName)?.Value ?? principal.FindFirst(ClaimTypes.GivenName)?.Value;
                if (!string.IsNullOrEmpty(givenName))
                {
                    claims[Claims.GivenName] = givenName;
                }

                var middleName = principal.FindFirst(Claims.MiddleName)?.Value;
                if (!string.IsNullOrEmpty(middleName))
                {
                    claims[Claims.MiddleName] = middleName;
                }

                var picture = principal.FindFirst(Claims.Picture)?.Value;
                if (!string.IsNullOrEmpty(picture))
                {
                    claims[Claims.Picture] = picture;
                }

                var updatedAtClaimValue = principal.FindFirst(Claims.UpdatedAt)?.Value;
                if (!string.IsNullOrEmpty(updatedAtClaimValue))
                {
                    claims[Claims.UpdatedAt] = long.Parse(updatedAtClaimValue, CultureInfo.InvariantCulture);
                }
            }

            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            claims[Claims.Subject] = principal.GetUserIdentifier();

            if (principal.HasScope(Scopes.Email))
            {
                var address = principal.FindFirst(Claims.Email)?.Value ?? principal.FindFirst(ClaimTypes.Email)?.Value;

                if (!string.IsNullOrEmpty(address))
                {
                    claims[Claims.Email] = address;

                    var status = principal.FindFirst(Claims.EmailVerified)?.Value;
                    if (!string.IsNullOrEmpty(status))
                    {
                        claims[Claims.EmailVerified] = bool.Parse(status);
                    }
                }
            }

            if (principal.HasScope(Scopes.Phone))
            {
                var phone = principal.FindFirst(Claims.PhoneNumber)?.Value ??
                            principal.FindFirst(ClaimTypes.MobilePhone)?.Value ??
                            principal.FindFirst(ClaimTypes.HomePhone)?.Value ??
                            principal.FindFirst(ClaimTypes.OtherPhone)?.Value;

                if (!string.IsNullOrEmpty(phone))
                {
                    claims[Claims.PhoneNumber] = phone;

                    var status = principal.FindFirst(Claims.PhoneNumberVerified)?.Value;
                    if (!string.IsNullOrEmpty(status))
                    {
                        claims[Claims.PhoneNumberVerified] = bool.Parse(status);
                    }
                }
            }

            if (principal.HasScope(Scopes.Roles))
            {
                var roles = principal.FindAll(Claims.Role)
                                     .Concat(principal.FindAll(ClaimTypes.Role))
                                     .Select(claim => claim.Value)
                                     .ToArray();

                if (roles.Length != 0)
                {
                    claims["roles"] = roles;
                }
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Ok(claims);
        }
    }
}

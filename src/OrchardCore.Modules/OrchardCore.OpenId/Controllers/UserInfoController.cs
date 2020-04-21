using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OpenIddict.Abstractions;
using OrchardCore.Modules;
using OrchardCore.OpenId.Filters;

namespace OrchardCore.OpenId.Controllers
{
    [Feature(OpenIdConstants.Features.Server)]
    [OpenIdController, SkipStatusCodePages]
    public class UserInfoController : Controller
    {
        private readonly IStringLocalizer S;

        public UserInfoController(IStringLocalizer<UserInfoController> localizer)
        {
            S = localizer;
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
            var principal = (await HttpContext.AuthenticateAsync(OpenIdConstants.Schemes.Userinfo))?.Principal;
            if (principal == null)
            {
                return Challenge(OpenIdConstants.Schemes.Userinfo);
            }

            // Ensure the access token represents a user and not an application.
            var type = principal.FindFirst(OpenIdConstants.Claims.EntityType)?.Value;
            if (!string.Equals(type, OpenIdConstants.EntityTypes.User))
            {
                return BadRequest(new OpenIdConnectResponse
                {
                    Error = OpenIddictConstants.Errors.InvalidRequest,
                    ErrorDescription = S["The userinfo endpoint can only be used with access tokens representing users."]
                });
            }

            var claims = new JObject();

            if (principal.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Profile))
            {
                var preferredUsername = principal.FindFirst(OpenIdConnectConstants.Claims.PreferredUsername)
                                                 ?.Value;
                if (!string.IsNullOrEmpty(preferredUsername))
                {
                    claims[OpenIdConnectConstants.Claims.PreferredUsername] = preferredUsername;
                }

                var name = principal.FindFirst(OpenIdConnectConstants.Claims.Name)?.Value ??
                              principal.FindFirst(ClaimTypes.Name)?.Value;

                if (!string.IsNullOrEmpty(name))
                {
                    claims[OpenIdConnectConstants.Claims.Name] = name;
                }

                var familyName = principal.FindFirst(OpenIdConnectConstants.Claims.FamilyName)?.Value ??
                           principal.FindFirst(ClaimTypes.Surname)?.Value;

                if (!string.IsNullOrEmpty(familyName))
                {
                    claims[OpenIdConnectConstants.Claims.FamilyName] = familyName;
                }

                var givenName = principal.FindFirst(OpenIdConnectConstants.Claims.GivenName)?.Value ??
                           principal.FindFirst(ClaimTypes.GivenName)?.Value;

                if (!string.IsNullOrEmpty(givenName))
                {
                    claims[OpenIdConnectConstants.Claims.GivenName] = givenName;
                }

                var middleName = principal.FindFirst(OpenIdConnectConstants.Claims.MiddleName)
                                          ?.Value;

                if (!string.IsNullOrEmpty(middleName))
                {
                    claims[OpenIdConnectConstants.Claims.MiddleName] = middleName;
                }

                var picture = principal.FindFirst(OpenIdConnectConstants.Claims.Picture)
                                       ?.Value;

                if (!string.IsNullOrEmpty(picture))
                {
                    claims[OpenIdConnectConstants.Claims.Picture] = picture;
                }

                var updatedAtClaimValue = principal.FindFirst(OpenIdConnectConstants.Claims.UpdatedAt)
                                       ?.Value;

                if (!string.IsNullOrEmpty(updatedAtClaimValue))
                {
                    claims[OpenIdConnectConstants.Claims.UpdatedAt] = long.Parse(updatedAtClaimValue, CultureInfo.InvariantCulture);
                }
            }

            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            claims[OpenIdConnectConstants.Claims.Subject] = principal.GetUserIdentifier();

            if (principal.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Email))
            {
                var address = principal.FindFirst(OpenIdConnectConstants.Claims.Email)?.Value ??
                              principal.FindFirst(ClaimTypes.Email)?.Value;

                if (!string.IsNullOrEmpty(address))
                {
                    claims[OpenIdConnectConstants.Claims.Email] = address;

                    var status = principal.FindFirst(OpenIdConnectConstants.Claims.EmailVerified)?.Value;
                    if (!string.IsNullOrEmpty(status))
                    {
                        claims[OpenIdConnectConstants.Claims.EmailVerified] = bool.Parse(status);
                    }
                }
            }

            if (principal.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Phone))
            {
                var phone = principal.FindFirst(OpenIdConnectConstants.Claims.PhoneNumber)?.Value ??
                            principal.FindFirst(ClaimTypes.MobilePhone)?.Value ??
                            principal.FindFirst(ClaimTypes.HomePhone)?.Value ??
                            principal.FindFirst(ClaimTypes.OtherPhone)?.Value;

                if (!string.IsNullOrEmpty(phone))
                {
                    claims[OpenIdConnectConstants.Claims.PhoneNumber] = phone;

                    var status = principal.FindFirst(OpenIdConnectConstants.Claims.PhoneNumberVerified)?.Value;
                    if (!string.IsNullOrEmpty(status))
                    {
                        claims[OpenIdConnectConstants.Claims.PhoneNumberVerified] = bool.Parse(status);
                    }
                }
            }

            if (principal.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIddictConstants.Scopes.Roles))
            {
                var roles = principal.FindAll(OpenIdConnectConstants.Claims.Role)
                                     .Concat(principal.FindAll(ClaimTypes.Role))
                                     .Select(claim => claim.Value)
                                     .ToArray<object>();

                if (roles.Length != 0)
                {
                    claims[OpenIddictConstants.Claims.Roles] = new JArray(roles);
                }
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Ok(claims);
        }
    }
}

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace OrchardCore.Users.Services
{

    public class EmailClaimsTransformer : IClaimsTransformation
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IOptions<IdentityOptions> _identityOptions;

        public EmailClaimsTransformer(
            UserManager<IUser> userManager,
            IOptions<IdentityOptions> identityOptions)
        {
            _userManager = userManager;
            _identityOptions = identityOptions;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal user)
        {
            if (user?.Identity is ClaimsIdentity identity)
            {
                var orchardUser = await _userManager.GetUserAsync(user);

                if (orchardUser != null)
                {
                    var email = await _userManager.GetEmailAsync(orchardUser);

                    if (email != null)
                    {
                        identity.AddClaim(new Claim("email", email));
                    }

                    if (await _userManager.IsEmailConfirmedAsync(orchardUser))
                    {
                        identity.AddClaim(new Claim("email_verified", "true"));
                    }
                }
            }
            return user;
        }
    }
}

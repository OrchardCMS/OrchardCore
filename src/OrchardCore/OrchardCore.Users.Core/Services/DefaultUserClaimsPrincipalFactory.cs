using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Custom implementation of  <see cref="IUserClaimsPrincipalFactory"/> adding email claims.
    /// </summary>
    public class DefaultUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
    {
        private readonly UserManager<User> _userManager;

        public DefaultUserClaimsPrincipalFactory(
            UserManager<User> userManager,
            IOptions<IdentityOptions> identityOptions) : base(userManager, identityOptions)
        {
            _userManager = userManager;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var claims = await base.GenerateClaimsAsync(user);

            var email = await _userManager.GetEmailAsync(user);

            if (email != null)
            {
                claims.AddClaim(new Claim("email", email));
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                claims.AddClaim(new Claim("email_verified", "true"));
            }

            return claims;
        }
    }
}

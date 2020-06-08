using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Custom implementation of  <see cref="IUserClaimsPrincipalFactory"/> adding email claims.
    /// </summary>
    public class DefaultUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IUser, IRole>
    {
        public DefaultUserClaimsPrincipalFactory(
            UserManager<IUser> userManager,
            RoleManager<IRole> roleManager,
            IOptions<IdentityOptions> identityOptions) : base(userManager, roleManager, identityOptions)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IUser user)
        {
            var claims = await base.GenerateClaimsAsync(user);

            var email = await UserManager.GetEmailAsync(user);
            if (email != null)
            {
                claims.AddClaim(new Claim("email", email));

                var confirmed = await UserManager.IsEmailConfirmedAsync(user);
                claims.AddClaim(new Claim("email_verified", confirmed ? bool.TrueString : bool.FalseString, ClaimValueTypes.Boolean));
            }

            return claims;
        }
    }
}

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Services;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Custom implementation of  <see cref="IUserClaimsPrincipalFactory"/> adding email claims.
    /// </summary>
    public class DefaultUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IUser>
    {
        private readonly IRoleProvider _roleProvider;
        private readonly UserManager<IUser> _userManager;
        private readonly IOptions<IdentityOptions> _identityOptions;

        public DefaultUserClaimsPrincipalFactory(
            IRoleProvider roleProvider,
            UserManager<IUser> userManager,
            IOptions<IdentityOptions> identityOptions) : base(userManager, identityOptions)
        {
            _roleProvider = roleProvider;
            _userManager = userManager;
            _identityOptions = identityOptions;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IUser user)
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

            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                foreach (var claim in await _roleProvider.GetRoleClaimsAsync(_userManager.NormalizeKey(role)))
                {
                    claims.AddClaim(claim);
                }
            }

            return claims;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Custom implementation of <see cref="IUserClaimsPrincipalFactory{TUser}"/> allowing adding claims by implementing the <see cref="IClaimsProvider"/>
    /// </summary>
    public class DefaultUserClaimsPrincipalProviderFactory : UserClaimsPrincipalFactory<IUser, IRole>
    {
        private readonly IEnumerable<IClaimsProvider> _claimsProviders;

        public DefaultUserClaimsPrincipalProviderFactory(
            UserManager<IUser> userManager,
            RoleManager<IRole> roleManager,
            IOptions<IdentityOptions> identityOptions,
            IEnumerable<IClaimsProvider> claimsProviders) : base(userManager, roleManager, identityOptions)
        {
            _claimsProviders = claimsProviders ?? Enumerable.Empty<IClaimsProvider>();
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IUser user)
        {
            var claims = await base.GenerateClaimsAsync(user);

            foreach (var claimsProvider in _claimsProviders)
            {
                await claimsProvider.GenerateAsync(user, claims);
            }

            return claims;
        }
    }
}

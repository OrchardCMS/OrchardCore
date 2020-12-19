using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Security;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Custom implementation of <see cref="IUserClaimsPrincipalFactory{TUser}"/> adding email claims.
    /// </summary>
    public class DefaultUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IUser, IRole>
    {

        private readonly IServiceProvider _serviceProvider;

        public DefaultUserClaimsPrincipalFactory(
            UserManager<IUser> userManager,
            RoleManager<IRole> roleManager,
            IOptions<IdentityOptions> identityOptions,
            IServiceProvider serviceProvider) : base(userManager, roleManager, identityOptions)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IUser user)
        {
            ClaimsIdentity claims = await base.GenerateClaimsAsync(user);

            var claimsProviders = _serviceProvider.GetRequiredService<IEnumerable<IClaimsProvider>>();

            foreach (IClaimsProvider claimsProvider in claimsProviders)
            {
                await claimsProvider.GenerateAsync(user, claims);
            }

            return claims;
        }
    }
}

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
            var claims = await base.GenerateClaimsAsync(user);

            var claimsProviders = _serviceProvider.GetRequiredService<IEnumerable<IClaimsProvider>>();

            foreach (IClaimsProvider claimsProvider in claimsProviders)
            {
                await claimsProvider.GenerateAsync(user, claims);
            }

            // Todo: In a future version the base implementation will generate the email claim if the user store is an 'IUserEmailStore',
            // so we will not have to add it here, and everywhere we are using the hardcoded "email" claim type, we will have to use the
            // new 'IdentityOptions.ClaimsIdentity.EmailClaimType' or at least its default value which is 'ClaimTypes.Email'.
            // Shouldn't this get removed and replaced by registering OrchardCore.Users.Services.EmailClaimsProvider into the IoC instead?

            var email = await UserManager.GetEmailAsync(user);
            if (!String.IsNullOrEmpty(email))
            {
                claims.AddClaim(new Claim("email", email));

                var confirmed = await UserManager.IsEmailConfirmedAsync(user);
                claims.AddClaim(new Claim("email_verified", confirmed ? bool.TrueString : bool.FalseString, ClaimValueTypes.Boolean));
            }

            return claims;
        }
    }
}

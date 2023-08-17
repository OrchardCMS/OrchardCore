using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security;

namespace OrchardCore.Users.Services
{
    /// <summary>
    /// Custom implementation of <see cref="IUserClaimsPrincipalFactory{TUser}"/> adding email claims.
    /// </summary>
    [Obsolete("The class 'DefaultUserClaimsPrincipalFactory' is obsolete, please implement 'IUserClaimsProvider' instead.")]
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

            // Todo: In a future version the base implementation will generate the email claim if the user store is an 'IUserEmailStore',
            // so we will not have to add it here, and everywhere we are using the hardcoded "email" claim type, we will have to use the
            // new 'IdentityOptions.ClaimsIdentity.EmailClaimType' or at least its default value which is 'ClaimTypes.Email'.

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

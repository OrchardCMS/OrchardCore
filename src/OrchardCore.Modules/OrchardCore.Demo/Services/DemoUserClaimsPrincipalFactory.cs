using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Demo.Models;
using OrchardCore.Entities;
using OrchardCore.Security;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Demo.Services
{
    internal class DemoUserClaimsPrincipalFactory : DefaultUserClaimsPrincipalFactory
    {
        public DemoUserClaimsPrincipalFactory(UserManager<IUser> userManager, RoleManager<IRole> roleManager,
            IOptions<IdentityOptions> identityOptions) : base(userManager, roleManager, identityOptions)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IUser user)
        {
            var claims = await base.GenerateClaimsAsync(user);

            var u = user as User;
            var profile = u.As<UserProfile>();

            claims.AddClaim(new Claim("preferred_username", user.UserName));

            var name = "";
            if (!string.IsNullOrEmpty(profile.FirstName))
            {
                claims.AddClaim(new Claim("given_name", profile.FirstName));
                name += profile.FirstName;
            }

            if (!string.IsNullOrEmpty(profile.LastName))
            {
                claims.AddClaim(new Claim("family_name", profile.LastName));
                name += $" {profile.LastName}";
            }

            if (!string.IsNullOrEmpty(name))
            {
                claims.AddClaim(new Claim("name", name));
            }

            if (profile.UpdatedAt != default)
            {
                claims.AddClaim(new Claim("updated_at", ConvertToUnixTimestamp(profile.UpdatedAt).ToString(CultureInfo.InvariantCulture)));
            }

            return claims;
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
    }
}

using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict;
using Orchard.Security;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Orchard.OpenId.Services
{
    public class OpenIdUserManager : OpenIddictUserManager<User>
    {
        private readonly RoleManager<Role> _roleManager;        

        public OpenIdUserManager(RoleManager<Role> roleManager, IServiceProvider services, IOpenIddictUserStore<User> store, IOptions<IdentityOptions> options, ILogger<OpenIddictUserManager<User>> logger, IPasswordHasher<User> hasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors) : base(services, store, options, logger, hasher, userValidators, passwordValidators, keyNormalizer, errors)
        {
            _roleManager = roleManager;
        }

        public override async Task<ClaimsIdentity> CreateIdentityAsync(User user, IEnumerable<string> scopes)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            var identity = new ClaimsIdentity(
                OpenIdConnectServerDefaults.AuthenticationScheme,
                Options.ClaimsIdentity.UserNameClaimType,
                Options.ClaimsIdentity.RoleClaimType);

            // Note: the name identifier is always included in both identity and
            // access tokens, even if an explicit destination is not specified.
            identity.AddClaim(ClaimTypes.NameIdentifier, await GetUserIdAsync(user));

            // Resolve the email address associated with the user if the underlying store supports it.
            var email = SupportsUserEmail ? await GetEmailAsync(user) : null;

            // Only add the name claim if the "profile" scope was granted.
            if (scopes.Contains(OpenIdConnectConstants.Scopes.Profile))
            {
                var username = await GetUserNameAsync(user);

                // Throw an exception if the username corresponds to the registered
                // email address and if the "email" scope has not been requested.
                if (!scopes.Contains(OpenIdConnectConstants.Scopes.Email) &&
                    !string.IsNullOrEmpty(email) &&
                     string.Equals(username, email, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("The 'email' scope is required.");
                }

                identity.AddClaim(ClaimTypes.Name, username,
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);
            }

            // Only add the email address if the "email" scope was granted.
            if (!string.IsNullOrEmpty(email) && scopes.Contains(OpenIdConnectConstants.Scopes.Email))
            {
                identity.AddClaim(ClaimTypes.Email, email,
                    OpenIdConnectConstants.Destinations.AccessToken,
                    OpenIdConnectConstants.Destinations.IdentityToken);
            }

            if (SupportsUserRole && scopes.Contains(OpenIddictConstants.Scopes.Roles))
            {
                foreach (var role in await GetRolesAsync(user))
                {
                    identity.AddClaim(identity.RoleClaimType, role,
                        OpenIdConnectConstants.Destinations.AccessToken,
                        OpenIdConnectConstants.Destinations.IdentityToken);
                    foreach (var claim in await _roleManager.GetClaimsAsync(await _roleManager.FindByNameAsync(role)))
                    {
                        identity.AddClaim(claim.Type, claim.Value, OpenIdConnectConstants.Destinations.IdentityToken);
                    }                   
                }
            }

            if (SupportsUserSecurityStamp)
            {
                var stamp = await GetSecurityStampAsync(user);

                if (!string.IsNullOrEmpty(stamp))
                {
                    identity.AddClaim(Options.ClaimsIdentity.SecurityStampClaimType, stamp,
                        OpenIdConnectConstants.Destinations.AccessToken,
                        OpenIdConnectConstants.Destinations.IdentityToken);
                }
            }

            return identity;
        }
    }
}

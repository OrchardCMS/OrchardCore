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
            var identity = await base.CreateIdentityAsync(user, scopes);
            
            if (SupportsUserRole && scopes.Contains(OpenIddictConstants.Scopes.Roles))
            {
                foreach (var role in await GetRolesAsync(user))
                {
                    foreach (var claim in await _roleManager.GetClaimsAsync(await _roleManager.FindByNameAsync(role)))
                    {
                        identity.AddClaim(claim.Type, claim.Value, OpenIdConnectConstants.Destinations.IdentityToken, OpenIdConnectConstants.Destinations.AccessToken);
                    }                   
                }
            }
                        
            return identity;
        }
    }
}

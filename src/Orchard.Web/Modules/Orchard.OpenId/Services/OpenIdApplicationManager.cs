using Orchard.OpenId.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenIddict;
using YesSql.Core.Services;
using Orchard.OpenId.Indexes;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Orchard.Security;
using AspNet.Security.OpenIdConnect.Server;
using AspNet.Security.OpenIdConnect.Extensions;

namespace Orchard.OpenId.Services
{
    public class OpenIdApplicationManager : OpenIddict.OpenIddictApplicationManager<OpenIdApplication>, IOpenIdApplicationManager
    {
        private readonly ISession _session;
        private readonly RoleManager<Role> _roleManager;
        private readonly IOpenIdApplicationStore _appStore;

        public OpenIdApplicationManager(IServiceProvider services,
            IOpenIdApplicationStore appstore, 
            RoleManager<Role> roleManager,
            ILogger<OpenIddictApplicationManager<OpenIdApplication>> logger, 
            ISession session) : base(services, appstore, logger)
        {
            _session = session;
            _roleManager = roleManager;
            _appStore = appstore;
        }

        public virtual Task<IEnumerable<OpenIdApplication>> GetAppsAsync(int skip, int pageSize)
        {
            return _session.QueryAsync<OpenIdApplication, OpenIdApplicationIndex>().Skip(skip).Take(pageSize).List();
        }

        public virtual Task<int> GetCount()
        {
            return _session.QueryAsync<OpenIdApplication, OpenIdApplicationIndex>().Count();
        }
        public virtual Task<IList<string>> GetRolesAsync(OpenIdApplication application)
        {
            return _appStore.GetRolesAsync(application,CancellationToken);
        }
    

        public async Task<ClaimsIdentity> CreateIdentityAsync(OpenIdApplication application, IEnumerable<string> scopes)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            var identity = new ClaimsIdentity(OpenIdConnectServerDefaults.AuthenticationScheme);
            identity.AddClaim(ClaimTypes.NameIdentifier, application.ClientId);
            identity.AddClaim(ClaimTypes.Name, await GetDisplayNameAsync(application),
                OpenIdConnectConstants.Destinations.AccessToken,
                OpenIdConnectConstants.Destinations.IdentityToken);

            if (scopes.Contains(OpenIddictConstants.Scopes.Roles))
            {
                foreach (var role in await GetRolesAsync(application))
                {
                    identity.AddClaim(identity.RoleClaimType, role,
                        OpenIdConnectConstants.Destinations.AccessToken,
                        OpenIdConnectConstants.Destinations.IdentityToken);

                    foreach (var claim in await _roleManager.GetClaimsAsync(await _roleManager.FindByNameAsync(role)))
                    {
                        identity.AddClaim(claim.Type, claim.Value, OpenIdConnectConstants.Destinations.AccessToken, OpenIdConnectConstants.Destinations.IdentityToken);
                    }
                }
            }

            return identity;
        }
    }
}

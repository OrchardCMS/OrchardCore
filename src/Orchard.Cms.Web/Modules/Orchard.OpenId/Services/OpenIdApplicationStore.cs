using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using YesSql.Core.Services;
using Orchard.OpenId.Models;
using Orchard.OpenId.Indexes;
using OpenIddict;

namespace Orchard.OpenId.Services
{
    public class OpenIdApplicationStore : IOpenIdApplicationStore
    {
        private readonly ISession _session;

        public OpenIdApplicationStore(ISession session)
        {
            _session = session;
        }
        
        public Task<string> CreateAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            _session.Save(application);

            return Task.FromResult(application.Id.ToString());
        }

        public Task<OpenIdApplication> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = int.Parse(identifier);
            return _session.GetAsync<OpenIdApplication>(key);
        }

        public virtual Task<OpenIdApplication> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _session.QueryAsync<OpenIdApplication, OpenIdApplicationIndex>(o => o.ClientId == identifier).FirstOrDefault();
        }

        public Task<OpenIdApplication> FindByLogoutRedirectUri(string url, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _session.QueryAsync<OpenIdApplication, OpenIdApplicationIndex>(o => o.LogoutRedirectUri == url).FirstOrDefault();
        }

        public Task<string> GetClientTypeAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult(application.Type.ToString().ToLower());
        }

        public Task<string> GetDisplayNameAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult(application.DisplayName);
        }

        public Task<string> GetHashedSecretAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult(application.ClientSecret);
        }

        public Task<string> GetRedirectUriAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult(application.RedirectUri);
        }

        public async Task<IEnumerable<string>> GetTokensAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return (await _session.QueryAsync<OpenIdToken,OpenIdTokenIndex>(t=>t.AppId == application.Id).List()).Select(o => o.Id.ToString()).AsEnumerable();
        }
        public Task<IList<string>> GetRolesAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult<IList<string>>(application.RoleNames);
        }
        public Task DeleteAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            _session.Delete(application);
            return Task.CompletedTask;
        }
    }
}

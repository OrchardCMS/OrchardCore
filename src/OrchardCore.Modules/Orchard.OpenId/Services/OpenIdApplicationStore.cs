using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Core;
using Orchard.OpenId.Indexes;
using Orchard.OpenId.Models;
using YesSql;

namespace Orchard.OpenId.Services
{
    public class OpenIdApplicationStore : IOpenIddictApplicationStore<OpenIdApplication>
    {
        private readonly ISession _session;

        public OpenIdApplicationStore(ISession session)
        {
            _session = session;
        }
        
        public async Task<OpenIdApplication> CreateAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(application);
            await _session.CommitAsync();

            return application;
        }

        public Task<OpenIdApplication> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _session.GetAsync<OpenIdApplication>(int.Parse(identifier, CultureInfo.InvariantCulture));
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

            return Task.FromResult(application.Type.ToString().ToLowerInvariant());
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

        public Task DeleteAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Delete(application);

            return _session.CommitAsync();
        }

        public Task<string> GetClientIdAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult(application.ClientId);
        }

        public Task<string> GetIdAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult(application.Id.ToString());
        }

        public Task<IEnumerable<string>> GetTokensAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetLogoutRedirectUriAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult(application.LogoutRedirectUri);
        }

        public Task SetClientTypeAsync(OpenIdApplication application, string type, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            
            application.Type = (ClientType) Enum.Parse(typeof(ClientType), type, true);

            return Task.CompletedTask;
        }

        public Task SetHashedSecretAsync(OpenIdApplication application, string hash, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.ClientSecret = hash;

            return Task.CompletedTask;
        }

        public Task UpdateAsync(OpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(application);

            return _session.CommitAsync();
        }

        public Task<IEnumerable<OpenIdApplication>> GetAppsAsync(int skip, int pageSize)
        {
            return _session.QueryAsync<OpenIdApplication, OpenIdApplicationIndex>().Skip(skip).Take(pageSize).List();
        }

        public Task<int> GetCount()
        {
            return _session.QueryAsync<OpenIdApplication, OpenIdApplicationIndex>().Count();
        }
    }
}

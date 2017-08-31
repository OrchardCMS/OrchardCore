using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Core;
using Orchard.OpenId.Indexes;
using Orchard.OpenId.Models;
using YesSql;

namespace Orchard.OpenId.Services
{
    public class OpenIdApplicationStore : IOpenIddictApplicationStore<OpenIdApplication>, IOpenIdApplicationRoleStore<OpenIdApplication>
    {
        private readonly ISession _session;

        public OpenIdApplicationStore(ISession session)
        {
            _session = session;
        }


        public Task<IEnumerable<OpenIdApplication>> GetAppsAsync(int skip, int pageSize)
        {
            return _session.Query<OpenIdApplication, OpenIdApplicationIndex>().Skip(skip).Take(pageSize).ListAsync();
        }

        public Task<int> GetCount()
        {
            return _session.Query<OpenIdApplication, OpenIdApplicationIndex>().CountAsync();
        }

        #region IOpenIddictApplicationStore<OpenIdApplication>
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

            return _session.Query<OpenIdApplication, OpenIdApplicationIndex>(o => o.ClientId == identifier).FirstOrDefaultAsync();
        }

        public Task<OpenIdApplication> FindByLogoutRedirectUri(string url, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<OpenIdApplication, OpenIdApplicationIndex>(o => o.LogoutRedirectUri == url).FirstOrDefaultAsync();
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

            application.Type = (ClientType)Enum.Parse(typeof(ClientType), type, true);

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
        #endregion

        #region IOpenIdApplicationRoleStore<OpenIdApplication>
        public Task AddToRoleAsync(OpenIdApplication application, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.RoleNames.Add(roleName);
            _session.Save(application);

            return Task.CompletedTask;
        }

        public Task RemoveFromRoleAsync(OpenIdApplication application, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.RoleNames.Remove(roleName);
            _session.Save(application);

            return Task.CompletedTask;
        }

        public Task<IList<string>> GetRolesAsync(OpenIdApplication application, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult<IList<string>>(application.RoleNames);
        }

        public Task<bool> IsInRoleAsync(OpenIdApplication application, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult(application.RoleNames.Contains(roleName, StringComparer.OrdinalIgnoreCase));
        }

        public async Task<IList<OpenIdApplication>> GetAppsInRoleAsync(string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            var apps = await _session.Query<OpenIdApplication, OpenIdApplicationByRoleNameIndex>(x => x.RoleName == roleName).ListAsync();
            return apps == null ? new List<OpenIdApplication>() : apps.ToList();
        }
        #endregion
    }
}

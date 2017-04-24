using System;
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
    public class OpenIdTokenStore : IOpenIddictTokenStore<OpenIdToken>
    {
        private readonly ISession _session;

        public OpenIdTokenStore(ISession session)
        {
            _session = session;
        }

        public async Task<OpenIdToken> CreateAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            cancellationToken.ThrowIfCancellationRequested();
            
            _session.Save(token);
            await _session.CommitAsync();

            return token;
        }

        public Task<OpenIdToken> CreateAsync(string type, string subject, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The token type cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.");
            }

            return CreateAsync(new OpenIdToken { Type = type, Subject = subject }, cancellationToken);
        }

        public Task<OpenIdToken> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = int.Parse(identifier, CultureInfo.InvariantCulture);
            return _session.QueryAsync<OpenIdToken, OpenIdTokenIndex>(o => o.TokenId == key).FirstOrDefault();
        }

        public async Task<OpenIdToken[]> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return (await _session.QueryAsync<OpenIdToken, OpenIdTokenIndex>(o => o.Subject == subject).List()).ToArray();
        }

        public Task<string> GetIdAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.Id.ToString());
        }

        public Task<string> GetSubjectAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.Subject);
        }

        public Task<string> GetTokenTypeAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Task.FromResult(token.Type);
        }

        public Task RevokeAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            _session.Delete(token);

            return _session.CommitAsync();
        }

        public Task SetAuthorizationAsync(OpenIdToken token, string identifier, CancellationToken cancellationToken)
        {
            // We may implement it in the future when adding "authorization/consent support" in the OpenID module
            throw new NotImplementedException();
        }

        public Task SetClientAsync(OpenIdToken token, string identifier, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(identifier))
            {
                throw new InvalidOperationException();
            }

            token.AppId = int.Parse(identifier, CultureInfo.InvariantCulture);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(token);

            return _session.CommitAsync();
        }
    }
}

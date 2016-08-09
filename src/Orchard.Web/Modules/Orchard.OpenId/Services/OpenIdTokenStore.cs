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
    public class OpenIdTokenStore : OpenIddict.IOpenIddictTokenStore<OpenIdToken>
    {
        private readonly ISession _session;

        public OpenIdTokenStore(ISession session)
        {
            _session = session;
        }
        
        public Task<string> CreateAsync(string type, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The token type cannot be null or empty.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var token = new OpenIdToken { Type = type };
            _session.Save(token);
            
            return Task.FromResult(token.Id.ToString());
        }

        Task<OpenIdToken> IOpenIddictTokenStore<OpenIdToken>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var key = int.Parse(identifier);
            return _session.GetAsync<OpenIdToken>(key);            
        }

        public Task RevokeAsync(OpenIdToken token, CancellationToken cancellationToken)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            _session.Delete(token);

            return Task.CompletedTask;
        }
    }
}

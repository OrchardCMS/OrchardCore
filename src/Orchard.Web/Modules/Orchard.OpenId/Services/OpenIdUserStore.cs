using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orchard.OpenId.Indexes;
using Orchard.Users.Models;
using YesSql.Core.Services;
using OpenIddict;
using Orchard.OpenId.Models;

namespace Orchard.OpenId.Services
{
    public class OpenIdUserStore : Orchard.Users.Services.UserStore, IOpenIddictUserStore<User>
    {
        private readonly ISession _session;

        public OpenIdUserStore(ISession session):base(session)
        {
            _session = session;
        }
        
        #region IOpenIddictUserStore<User>        

        public Task<string> CreateTokenAsync(User user, string type, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The token type cannot be null or empty.");
            }
            
            var token = new OpenIdToken { Type = type, UserId = user.Id };
            _session.Save(token);

            return Task.FromResult(token.Id.ToString());
        }

        public async Task<string> CreateTokenAsync(User user, string client, string type, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The token type cannot be null or empty.");
            }

            var application = await _session.QueryAsync<OpenIdApplication, OpenIdApplicationIndex>(o => o.ClientId == client).FirstOrDefault();
            if (application == null)
            {
                throw new InvalidOperationException("The application cannot be found in the database.");
            }

            var token = new OpenIdToken { Type = type, AppId = application.Id, UserId = user.Id };
            
            _session.Save(token);
            
            return token.Id.ToString();
        }

        public async Task<IEnumerable<string>> GetTokensAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return (await _session.QueryAsync<OpenIdToken, OpenIdTokenIndex>(u => u.UserId == user.Id).List()).Select(u=>u.Id.ToString());
        }
        #endregion
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Core;
using Orchard.OpenId.Models;

namespace Orchard.OpenId.Services
{
    public class OpenIdAuthorizationStore : IOpenIddictAuthorizationStore<OpenIdAuthorization>
    {
        public Task<OpenIdAuthorization> CreateAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<OpenIdAuthorization> FindAsync(string subject, string client, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<OpenIdAuthorization> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetIdAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetSubjectAsync(OpenIdAuthorization authorization, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

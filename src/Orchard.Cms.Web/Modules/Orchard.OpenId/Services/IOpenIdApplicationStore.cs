using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orchard.OpenId.Models;

namespace Orchard.OpenId.Services
{
    public interface IOpenIdApplicationStore : OpenIddict.IOpenIddictApplicationStore<OpenIdApplication>
    {
        Task<IList<string>> GetRolesAsync(OpenIdApplication application, CancellationToken cancellationToken);
        Task DeleteAsync(OpenIdApplication application, CancellationToken cancellationToken);
    }
}
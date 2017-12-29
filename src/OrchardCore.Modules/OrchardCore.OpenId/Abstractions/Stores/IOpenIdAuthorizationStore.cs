using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.Abstractions.Stores
{
    public interface IOpenIdAuthorizationStore : IOpenIddictAuthorizationStore<IOpenIdAuthorization>
    {
        Task<IOpenIdAuthorization> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken);
        Task<string> GetPhysicalIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken);
    }
}
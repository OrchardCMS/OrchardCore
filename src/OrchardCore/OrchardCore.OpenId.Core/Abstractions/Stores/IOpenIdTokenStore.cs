using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.Abstractions.Stores
{
    public interface IOpenIdTokenStore : IOpenIddictTokenStore<IOpenIdToken>
    {
        Task<IOpenIdToken> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken);
        Task<string> GetPhysicalIdAsync(IOpenIdToken token, CancellationToken cancellationToken);
    }
}
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.Abstractions.Stores
{
    public interface IOpenIdScopeStore : IOpenIddictScopeStore<IOpenIdScope>
    {
        Task<IOpenIdScope> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken);
        Task<string> GetPhysicalIdAsync(IOpenIdScope scope, CancellationToken cancellationToken);
    }
}
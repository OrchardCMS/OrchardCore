using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

namespace OrchardCore.OpenId.Abstractions.Stores
{
    public interface IOpenIdScopeStore<TScope> : IOpenIddictScopeStore<TScope> where TScope : class
    {
        ValueTask<TScope> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken);
        ValueTask<string> GetPhysicalIdAsync(TScope scope, CancellationToken cancellationToken);
    }
}

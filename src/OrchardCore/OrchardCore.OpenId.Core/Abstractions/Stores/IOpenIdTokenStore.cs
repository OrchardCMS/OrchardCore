using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

namespace OrchardCore.OpenId.Abstractions.Stores
{
    public interface IOpenIdTokenStore<TToken> : IOpenIddictTokenStore<TToken> where TToken : class
    {
        ValueTask<TToken> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken);
        ValueTask<string> GetPhysicalIdAsync(TToken token, CancellationToken cancellationToken);
    }
}

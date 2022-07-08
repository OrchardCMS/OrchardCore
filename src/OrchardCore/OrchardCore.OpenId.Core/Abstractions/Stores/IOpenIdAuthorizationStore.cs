using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

namespace OrchardCore.OpenId.Abstractions.Stores
{
    public interface IOpenIdAuthorizationStore<TAuthorization> : IOpenIddictAuthorizationStore<TAuthorization> where TAuthorization : class
    {
        ValueTask<TAuthorization> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken);
        ValueTask<string> GetPhysicalIdAsync(TAuthorization authorization, CancellationToken cancellationToken);
    }
}

using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

namespace OrchardCore.OpenId.Abstractions.Managers
{
    /// <summary>
    /// Provides methods allowing to manage the tokens stored in the store.
    /// Note: this interface is not meant to be implemented by custom managers,
    /// that should inherit from the generic OpenIdTokenManager class.
    /// It is primarily intended to be used by services that cannot easily
    /// depend on the generic token manager. The actual token entity type is
    /// automatically determined at runtime based on the OpenIddict core options.
    /// </summary>
    public interface IOpenIdTokenManager : IOpenIddictTokenManager
    {
        ValueTask<object> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default);
        ValueTask<string> GetPhysicalIdAsync(object token, CancellationToken cancellationToken = default);
    }
}

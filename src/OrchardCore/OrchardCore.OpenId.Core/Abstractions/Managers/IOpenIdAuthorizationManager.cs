using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

namespace OrchardCore.OpenId.Abstractions.Managers
{
    /// <summary>
    /// Provides methods allowing to manage the authorizations stored in the store.
    /// Note: this interface is not meant to be implemented by custom managers,
    /// that should inherit from the generic OpenIdAuthorizationManager class.
    /// It is primarily intended to be used by services that cannot easily depend
    /// on the generic authorization manager. The actual authorization entity type
    /// is automatically determined at runtime based on the OpenIddict core options.
    /// </summary>
    public interface IOpenIdAuthorizationManager : IOpenIddictAuthorizationManager
    {
        ValueTask<object> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default);
        ValueTask<string> GetPhysicalIdAsync(object authorization, CancellationToken cancellationToken = default);
    }
}

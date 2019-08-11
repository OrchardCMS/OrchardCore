using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

namespace OrchardCore.OpenId.Abstractions.Managers
{
    /// <summary>
    /// Provides methods allowing to manage the applications stored in the store.
    /// Note: this interface is not meant to be implemented by custom managers,
    /// that should inherit from the generic OpenIdApplicationManager class.
    /// It is primarily intended to be used by services that cannot easily depend
    /// on the generic application manager. The actual application entity type
    /// is automatically determined at runtime based on the OpenIddict core options.
    /// </summary>
    public interface IOpenIdApplicationManager : IOpenIddictApplicationManager
    {
        Task AddToRoleAsync(object application, string role, CancellationToken cancellationToken = default);
        Task<object> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default);
        ValueTask<string> GetPhysicalIdAsync(object application, CancellationToken cancellationToken = default);
        ValueTask<ImmutableArray<string>> GetRolesAsync(object application, CancellationToken cancellationToken = default);
        Task<bool> IsInRoleAsync(object application, string role, CancellationToken cancellationToken = default);
        Task<ImmutableArray<object>> ListInRoleAsync(string role, CancellationToken cancellationToken = default);
        Task RemoveFromRoleAsync(object application, string role, CancellationToken cancellationToken = default);
    }
}
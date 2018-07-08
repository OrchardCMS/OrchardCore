using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

namespace OrchardCore.OpenId.Abstractions.Stores
{
    public interface IOpenIdApplicationStore<TApplication> : IOpenIddictApplicationStore<TApplication> where TApplication : class
    {
        Task<TApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken);
        ValueTask<string> GetPhysicalIdAsync(TApplication application, CancellationToken cancellationToken);

        ValueTask<ImmutableArray<string>> GetRolesAsync(TApplication application, CancellationToken cancellationToken);
        Task<ImmutableArray<TApplication>> ListInRoleAsync(string role, CancellationToken cancellationToken);
        Task SetRolesAsync(TApplication application, ImmutableArray<string> roles, CancellationToken cancellationToken);
    }
}
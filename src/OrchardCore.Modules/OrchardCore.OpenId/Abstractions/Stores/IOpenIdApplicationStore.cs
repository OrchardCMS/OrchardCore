using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;

namespace OrchardCore.OpenId.Abstractions.Stores
{
    public interface IOpenIdApplicationStore : IOpenIddictApplicationStore<IOpenIdApplication>
    {
        Task<IOpenIdApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken);
        Task<string> GetPhysicalIdAsync(IOpenIdApplication application, CancellationToken cancellationToken);

        Task<bool> IsConsentRequiredAsync(IOpenIdApplication application, CancellationToken cancellationToken);
        Task SetConsentRequiredAsync(IOpenIdApplication application, bool value, CancellationToken cancellationToken);

        Task<ImmutableArray<string>> GetRolesAsync(IOpenIdApplication application, CancellationToken cancellationToken);
        Task<ImmutableArray<IOpenIdApplication>> ListInRoleAsync(string role, CancellationToken cancellationToken);
        Task SetRolesAsync(IOpenIdApplication application, ImmutableArray<string> roles, CancellationToken cancellationToken);
    }
}
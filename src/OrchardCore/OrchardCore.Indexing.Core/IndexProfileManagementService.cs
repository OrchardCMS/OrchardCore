using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

/// <summary>
/// Shares provider coordination between index administration, recipes and management endpoints.
/// </summary>
public sealed class IndexProfileManagementService : IIndexProfileManagementService
{
    private readonly IIndexProfileManager _profiles;
    private readonly IServiceProvider _services;

    /// <summary>
    /// Creates the coordinator using the tenant's profile manager and keyed provider services.
    /// </summary>
    public IndexProfileManagementService(IIndexProfileManager profiles, IServiceProvider services)
    {
        _profiles = profiles;
        _services = services;
    }

    /// <inheritdoc />
    public async Task<IndexProfileManagementResult> CreateAsync(IndexProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var provider = _services.GetKeyedService<IIndexManager>(profile.ProviderName);
        if (provider is null)
        {
            return IndexProfileManagementResult.ProviderUnavailable;
        }

        var validation = await _profiles.ValidateAsync(profile);
        if (!validation.Succeeded)
        {
            throw new IndexProfileValidationException(validation.Errors);
        }

        // Creating handlers must populate provider metadata before provider creation.
        await _profiles.CreateAsync(profile);
        if (!await provider.CreateAsync(profile))
        {
            return await _profiles.DeleteAsync(profile)
                ? IndexProfileManagementResult.ProviderRejected
                : IndexProfileManagementResult.LocalDeleteFailed;
        }

        await _profiles.SynchronizeAsync(profile);
        return IndexProfileManagementResult.Success;
    }

    /// <inheritdoc />
    public async Task<IndexProfileManagementResult> DeleteAsync(IndexProfile profile, bool force = false)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var provider = _services.GetKeyedService<IIndexManager>(profile.ProviderName);
        if (force)
        {
            if (provider is not null)
            {
                await provider.DeleteAsync(profile);
            }
        }
        else
        {
            if (provider is null)
            {
                return IndexProfileManagementResult.ProviderUnavailable;
            }

            if (await provider.ExistsAsync(profile.IndexFullName) && !await provider.DeleteAsync(profile))
            {
                return IndexProfileManagementResult.ProviderRejected;
            }
        }

        return await _profiles.DeleteAsync(profile)
            ? IndexProfileManagementResult.Success
            : IndexProfileManagementResult.LocalDeleteFailed;
    }
}

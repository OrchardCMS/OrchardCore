using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Enables and configures remote management in a running tenant, including its feature dependencies.
/// </summary>
public static class RemoteManagementProvisioning
{
    /// <summary>
    /// Configures the CLI feature and optionally creates an administrative application.
    /// Returns false when the feature is unavailable, for example because of a feature profile.
    /// The caller must authorize tenant administration before invoking this method.
    /// </summary>
    public static async Task<bool> ConfigureAsync(IShellHost shellHost, ShellSettings settings, RemoteManagementClientCredentials credentials = null)
    {
        ArgumentNullException.ThrowIfNull(shellHost);
        ArgumentNullException.ThrowIfNull(settings);

        const string featureId = "OrchardCore.RemoteManagement.Cli";
        var scope = await shellHost.GetScopeAsync(settings);
        await scope.UsingAsync(async childScope =>
        {
            var manager = childScope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var feature = (await manager.GetAvailableFeaturesAsync()).FirstOrDefault(feature => feature.Id == featureId);
            if (feature is not null && !(await manager.GetEnabledFeaturesAsync()).Any(candidate => candidate.Id == featureId))
            {
                await manager.EnableFeaturesAsync([feature], force: true);
            }
        });

        var configured = false;
        var configurationScope = await shellHost.GetScopeAsync(settings);
        await configurationScope.UsingAsync(async childScope =>
        {
            if (!(await childScope.ServiceProvider.GetRequiredService<IShellFeaturesManager>().GetEnabledFeaturesAsync())
                .Any(feature => feature.Id == featureId))
            {
                return;
            }

            await childScope.ServiceProvider.GetRequiredService<IRemoteManagementTenantConfigurationService>().ConfigureAsync();
            await childScope.ServiceProvider.GetRequiredService<IRemoteManagementCliConfigurationService>().ConfigureAsync();
            if (credentials is not null)
            {
                await childScope.ServiceProvider.GetRequiredService<IRemoteManagementClientProvisioningService>().CreateAsync(credentials);
            }

            configured = true;
        });

        if (configured)
        {
            await shellHost.ReloadShellContextAsync(settings);
        }

        return configured;
    }
}

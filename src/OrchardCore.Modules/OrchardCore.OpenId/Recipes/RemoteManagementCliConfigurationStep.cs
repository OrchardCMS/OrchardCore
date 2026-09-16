using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.OpenId.Recipes;

/// <summary>
/// Configures the Pomi client and shared authentication for the CLI feature.
/// </summary>
public sealed class RemoteManagementCliConfigurationStep : NamedRecipeStepHandler
{
    private readonly IRemoteManagementTenantConfigurationService _tenantConfiguration;
    private readonly IRemoteManagementCliConfigurationService _cliConfiguration;

    public RemoteManagementCliConfigurationStep(IRemoteManagementTenantConfigurationService tenantConfiguration,
        IRemoteManagementCliConfigurationService cliConfiguration) : base("RemoteManagementCliConfiguration")
    {
        _tenantConfiguration = tenantConfiguration;
        _cliConfiguration = cliConfiguration;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        await _tenantConfiguration.ConfigureAsync();
        await _cliConfiguration.ConfigureAsync();
    }
}

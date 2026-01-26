using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Secrets.Deployment;
using OrchardCore.Secrets.Recipes;
using OrchardCore.Secrets.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Secrets;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSecrets();
        services.AddSecretStore<DatabaseSecretStore>();

        services.AddPermissionProvider<SecretsPermissionProvider>();
        services.AddNavigationProvider<AdminMenu>();

        services.AddDeployment<SecretsDeploymentSource, SecretsDeploymentStep, SecretsDeploymentStepDriver>();
        services.AddRecipeExecutionStep<SecretsRecipeStep>();
    }
}

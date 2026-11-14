using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Secrets.Deployment;
using OrchardCore.Secrets.Drivers;
using OrchardCore.Secrets.Filters;
using OrchardCore.Secrets.Providers;
using OrchardCore.Secrets.Recipes;
using OrchardCore.Secrets.Services;
using OrchardCore.Secrets.ViewModels;
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
        services.Configure<MvcOptions>(options => options.Filters.Add<SecretExpirationWarningFilter>());

        // Register the secret types, the cases of the Secret union.
        services.AddSecretType<TextSecretTypeProvider>();
        services.AddSecretType<RsaKeySecretTypeProvider>();
        services.AddSecretType<X509SecretTypeProvider>();

        // Display drivers for secret types
        services.AddDisplayDriver<TextSecret, TextSecretDisplayDriver>();
        services.AddDisplayDriver<RsaKeySecret, RsaSecretDisplayDriver>();
        services.AddDisplayDriver<X509Secret, X509SecretDisplayDriver>();

        // Build the editors of a secret and of the move of secrets to another store.
        services.AddDisplayDriver<SecretEditViewModel, SecretEditDisplayDriver>();
        services.AddDisplayDriver<SecretStoreViewModel, SecretStoreDisplayDriver>();

        services.AddDeployment<SecretsDeploymentSource, SecretsDeploymentStep, SecretsDeploymentStepDriver>();
        services.AddRecipeExecutionStep<SecretsRecipeStep>();
    }
}

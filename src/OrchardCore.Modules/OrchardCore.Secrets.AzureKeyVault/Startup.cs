using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.Secrets.AzureKeyVault;

public sealed class Startup : StartupBase
{
    private const string AzureKeyVaultFeature = "OrchardCore.Secrets.AzureKeyVault";

    private readonly string _tenant;
    private readonly IShellConfiguration _configuration;
    private readonly ILogger _logger;

    public Startup(ShellSettings shellSettings, IShellConfiguration configuration, ILogger<Startup> logger)
    {
        _tenant = shellSettings.Name;
        _configuration = configuration;
        _logger = logger;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        var options = new AzureKeyVaultSecretStoreOptions();
        new AzureKeyVaultSecretStoreOptionsSetup(_configuration).Configure(options);

        var validationErrors = options.Validate(services);

        if (validationErrors.Any())
        {
            _logger.LogError(
                "The '{Feature}' feature is not active on tenant '{TenantName}': {Errors}",
                AzureKeyVaultFeature,
                _tenant,
                string.Join(' ', validationErrors.Select(error => error.ErrorMessage)));
        }
        else
        {
            services.AddTransient<IConfigureOptions<AzureKeyVaultSecretStoreOptions>, AzureKeyVaultSecretStoreOptionsSetup>();
            services.AddKeyedSingleton<SecretClient>(AzureKeyVaultSecretStore.SecretClientServiceKey,
                (provider, _) => provider.GetRequiredKeyedService<SecretClient>(options.AzureClient));
            services.AddSecretStore<AzureKeyVaultSecretStore>();
        }
    }
}

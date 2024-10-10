using System.Text;
using Microsoft.Extensions.Logging;
using OrchardCore.Abstractions.Setup;
using OrchardCore.AutoSetup.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Setup.Services;

namespace OrchardCore.AutoSetup.Services;

public class AutoSetupService : IAutoSetupService
{
    private readonly IShellHost _shellHost;
    private readonly IShellSettingsManager _shellSettingsManager;
    private readonly ISetupService _setupService;
    private readonly ILogger _logger;

    public AutoSetupService(
        IShellHost shellHost,
        IShellSettingsManager shellSettingsManager,
        ISetupService setupService,
        ILogger<AutoSetupService> logger
        )
    {
        _shellHost = shellHost;
        _shellSettingsManager = shellSettingsManager;
        _setupService = setupService;
        _logger = logger;
    }
    
    public async Task<bool> SetupTenantAsync(TenantSetupOptions setupOptions, ShellSettings shellSettings)
    {
        var setupContext = await GetSetupContextAsync(setupOptions, shellSettings);

        _logger.LogInformation("The AutoSetup is initializing the site.");

        await _setupService.SetupAsync(setupContext);

        if (setupContext.Errors.Count == 0)
        {
            _logger.LogInformation("The AutoSetup successfully provisioned the site '{SiteName}'.", setupOptions.SiteName);

            return true;
        }

        var stringBuilder = new StringBuilder();
        foreach (var error in setupContext.Errors)
        {
            stringBuilder.AppendLine($"{error.Key} : '{error.Value}'");
        }

        _logger.LogError("The AutoSetup failed installing the site '{SiteName}' with errors: {Errors}.", setupOptions.SiteName, stringBuilder);

        return false;
    }
   
    public async Task<ShellSettings> CreateTenantSettingsAsync(TenantSetupOptions setupOptions)
    {
        using var shellSettings = _shellSettingsManager
            .CreateDefaultSettings()
            .AsUninitialized()
            .AsDisposable();

        shellSettings.Name = setupOptions.ShellName;
        shellSettings.RequestUrlHost = setupOptions.RequestUrlHost;
        shellSettings.RequestUrlPrefix = setupOptions.RequestUrlPrefix;
        shellSettings["ConnectionString"] = setupOptions.DatabaseConnectionString;
        shellSettings["TablePrefix"] = setupOptions.DatabaseTablePrefix;
        shellSettings["Schema"] = setupOptions.DatabaseSchema;
        shellSettings["DatabaseProvider"] = setupOptions.DatabaseProvider;
        shellSettings["Secret"] = Guid.NewGuid().ToString();
        shellSettings["RecipeName"] = setupOptions.RecipeName;
        shellSettings["FeatureProfile"] = setupOptions.FeatureProfile;

        await _shellHost.UpdateShellSettingsAsync(shellSettings);

        return shellSettings;
    }
    
    public async Task<SetupContext> GetSetupContextAsync(TenantSetupOptions options, ShellSettings shellSettings)
    {
        var recipes = await _setupService.GetSetupRecipesAsync();

        var recipe = recipes.SingleOrDefault(r => r.Name == options.RecipeName);

        var setupContext = new SetupContext
        {
            Recipe = recipe,
            ShellSettings = shellSettings,
            Errors = new Dictionary<string, string>()
        };

        if (shellSettings.IsDefaultShell())
        {
            // The 'Default' shell is first created by the infrastructure,
            // so the following 'Autosetup' options need to be passed.
            shellSettings.RequestUrlHost = options.RequestUrlHost;
            shellSettings.RequestUrlPrefix = options.RequestUrlPrefix;
        }

        setupContext.Properties[SetupConstants.AdminEmail] = options.AdminEmail;
        setupContext.Properties[SetupConstants.AdminPassword] = options.AdminPassword;
        setupContext.Properties[SetupConstants.AdminUsername] = options.AdminUsername;
        setupContext.Properties[SetupConstants.DatabaseConnectionString] = options.DatabaseConnectionString;
        setupContext.Properties[SetupConstants.DatabaseProvider] = options.DatabaseProvider;
        setupContext.Properties[SetupConstants.DatabaseTablePrefix] = options.DatabaseTablePrefix;
        setupContext.Properties[SetupConstants.DatabaseSchema] = options.DatabaseSchema;
        setupContext.Properties[SetupConstants.SiteName] = options.SiteName;
        setupContext.Properties[SetupConstants.SiteTimeZone] = options.SiteTimeZone;

        return setupContext;
    }
}

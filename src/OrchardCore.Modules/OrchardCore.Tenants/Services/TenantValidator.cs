using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Services;

public partial class TenantValidator : ITenantValidator
{
    private readonly IShellHost _shellHost;
    private readonly IShellSettingsManager _shellSettingsManager;
    private readonly IFeatureProfilesService _featureProfilesService;
    private readonly IDbConnectionValidator _dbConnectionValidator;
    private readonly TenantsOptions _tenantsOptions;
    private readonly IEnumerable<DatabaseProvider> _databaseProviders;
    private readonly TenantDatabasePatternResolver _tenantDatabasePatternResolver;

    protected readonly IStringLocalizer S;

    public TenantValidator(
        IShellHost shellHost,
        IShellSettingsManager shellSettingsManager,
        IFeatureProfilesService featureProfilesService,
        IDbConnectionValidator dbConnectionValidator,
        IOptions<TenantsOptions> tenantsOptions,
        IEnumerable<DatabaseProvider> databaseProviders,
        TenantDatabasePatternResolver tenantDatabasePatternResolver,
        IStringLocalizer<TenantValidator> stringLocalizer)
    {
        _shellHost = shellHost;
        _shellSettingsManager = shellSettingsManager;
        _featureProfilesService = featureProfilesService;
        _dbConnectionValidator = dbConnectionValidator;
        _tenantsOptions = tenantsOptions.Value;
        _databaseProviders = databaseProviders;
        _tenantDatabasePatternResolver = tenantDatabasePatternResolver;
        S = stringLocalizer;
    }

    public async Task<IEnumerable<ModelError>> ValidateAsync(TenantModelBase model)
    {
        var errors = new List<ModelError>();

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            errors.Add(new ModelError(nameof(model.Name), S["The tenant name is mandatory."]));
        }

        if (model.FeatureProfiles is not null && model.FeatureProfiles.Length > 0)
        {
            var featureProfiles = await _featureProfilesService.GetFeatureProfilesAsync();

            foreach (var featureProfile in model.FeatureProfiles)
            {
                if (!featureProfiles.ContainsKey(featureProfile))
                {
                    errors.Add(new ModelError(nameof(model.FeatureProfiles), S["The feature profile does not exist."]));
                }
            }
        }

        if (!string.IsNullOrEmpty(model.Name) && !TenantNameRuleRegex().IsMatch(model.Name))
        {
            errors.Add(new ModelError(nameof(model.Name), S["Invalid tenant name. Must contain characters only and no spaces."]));
        }

        _ = _shellHost.TryGetSettings(model.Name, out var existingShellSettings);

        if (!string.IsNullOrWhiteSpace(model.RequestUrlPrefix) && model.RequestUrlPrefix.Contains('/'))
        {
            errors.Add(new ModelError(nameof(model.RequestUrlPrefix), S["The url prefix can not contain more than one segment."]));
        }

        if (_shellHost.GetAllSettings().Any(settings =>
            settings != existingShellSettings &&
            settings.HasUrlPrefix(model.RequestUrlPrefix) &&
            settings.HasUrlHost(model.RequestUrlHost)))
        {
            errors.Add(new ModelError(nameof(model.RequestUrlPrefix), S["A tenant with the same host and prefix already exists."]));
        }

        ShellSettings shellSettings = null;
        if (model.IsNewTenant)
        {
            if (existingShellSettings is null)
            {
                // Set the settings to be validated.
                shellSettings = _shellSettingsManager
                    .CreateDefaultSettings()
                    .AsUninitialized()
                    .AsDisposable();

                shellSettings.Name = model.Name;
            }
            else if (existingShellSettings.IsDefaultShell())
            {
                errors.Add(new ModelError(nameof(model.Name), S["The tenant name is in conflict with the 'Default' tenant."]));
            }
            else
            {
                errors.Add(new ModelError(nameof(model.Name), S["A tenant with the same name already exists."]));
            }
        }
        else if (existingShellSettings is null)
        {
            errors.Add(new ModelError(nameof(model.Name), S["The existing tenant to be validated was not found."]));
        }
        else if (existingShellSettings.IsSetupable())
        {
            // Database settings may still have been changed.
            shellSettings = existingShellSettings;
        }

        if (!string.IsNullOrWhiteSpace(model.Name))
        {
            var databasePatternResolution = _tenantDatabasePatternResolver.Apply(model);

            if (!string.IsNullOrEmpty(databasePatternResolution.TablePrefixError))
            {
                errors.Add(new ModelError(nameof(model.TablePrefix), databasePatternResolution.TablePrefixError));
            }

            if (!string.IsNullOrEmpty(databasePatternResolution.SchemaError))
            {
                errors.Add(new ModelError(nameof(model.Schema), databasePatternResolution.SchemaError));
            }
        }

        if ((model.IsNewTenant || existingShellSettings?.IsSetupable() == true) &&
            IsTablePrefixRequired(model.DatabaseProvider) &&
            string.IsNullOrWhiteSpace(model.TablePrefix))
        {
            errors.Add(new ModelError(nameof(model.TablePrefix), S["A table prefix is required."]));
        }

        if (ProviderSupportsTablePrefix(model.DatabaseProvider) &&
            !string.IsNullOrWhiteSpace(model.TablePrefix) &&
            !TenantDatabasePatternResolver.IsSqlIdentifier(model.TablePrefix))
        {
            errors.Add(new ModelError(nameof(model.TablePrefix), S["The table prefix must be a valid SQL identifier using only letters, numbers, and underscores, and it must start with a letter or underscore."]));
        }

        if (!string.IsNullOrWhiteSpace(model.Schema) &&
            !TenantDatabasePatternResolver.IsSqlIdentifier(model.Schema))
        {
            errors.Add(new ModelError(nameof(model.Schema), S["The table schema must be a valid SQL identifier using only letters, numbers, and underscores, and it must start with a letter or underscore."]));
        }

        if (shellSettings is not null)
        {
            // A newly loaded settings from the configuration should be disposed.
            using var disposable = existingShellSettings is null ? shellSettings : null;

            // Skip connection validation if no connection string was provided and the
            // provider requires one. The admin may intentionally leave it blank so the
            // person setting up the tenant can supply one during setup.
            var provider = _databaseProviders.FirstOrDefault(p => p.Value == model.DatabaseProvider);
            if (provider is null || !provider.HasConnectionString || !string.IsNullOrWhiteSpace(model.ConnectionString))
            {
                var validationContext = new DbConnectionValidatorContext(shellSettings, model);
                await ValidateConnectionAsync(validationContext, errors);
            }
        }

        return errors;
    }

    private async Task ValidateConnectionAsync(DbConnectionValidatorContext validationContext, List<ModelError> errors)
    {
        switch (await _dbConnectionValidator.ValidateAsync(validationContext))
        {
            case DbConnectionValidatorResult.UnsupportedProvider:
                errors.Add(new ModelError(nameof(
                    TenantViewModel.DatabaseProvider),
                    S["The provided database provider is not supported."]));
                break;

            case DbConnectionValidatorResult.InvalidConnection:
                errors.Add(new ModelError(
                    nameof(TenantViewModel.ConnectionString),
                    S["The provided connection string is invalid or server is unreachable."]));
                break;

            case DbConnectionValidatorResult.InvalidCertificate:
                errors.Add(new ModelError(
                    nameof(TenantViewModel.ConnectionString),
                    S["The security certificate on the server is from a non-trusted source (the certificate issuing authority isn't listed as a trusted authority in Trusted Root Certification Authorities on the client machine). In a development environment, you have the option to use the '{0}' parameter in your connection string to bypass the validation performed by the certificate authority.", "TrustServerCertificate=True"]));
                break;

            case DbConnectionValidatorResult.DocumentTableFound:
                if (validationContext.DatabaseProvider == DatabaseProviderValue.Sqlite)
                {
                    errors.Add(new ModelError(
                        string.Empty,
                        S["The related database file is already in use."]));
                    break;
                }

                errors.Add(new ModelError(
                    nameof(TenantViewModel.TablePrefix),
                    S["The provided database, table prefix and schema are already in use."]));
                break;
        }
    }

    private bool IsTablePrefixRequired(string databaseProvider) =>
        _tenantsOptions.RequireTablePrefix &&
        ProviderSupportsTablePrefix(databaseProvider);

    private bool ProviderSupportsTablePrefix(string databaseProvider) =>
        _databaseProviders.Any(provider =>
            provider.HasTablePrefix &&
            string.Equals(provider.Value, databaseProvider, StringComparison.OrdinalIgnoreCase));

    [GeneratedRegex(@"^\w+$")]
    private static partial Regex TenantNameRuleRegex();
}

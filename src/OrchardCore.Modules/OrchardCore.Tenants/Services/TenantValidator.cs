using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Tenants.Models;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Services
{
    public class TenantValidator : ITenantValidator
    {
        private readonly IShellHost _shellHost;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IFeatureProfilesService _featureProfilesService;
        private readonly IDbConnectionValidator _dbConnectionValidator;
        protected readonly IStringLocalizer S;

        public TenantValidator(
            IShellHost shellHost,
            IShellSettingsManager shellSettingsManager,
            IFeatureProfilesService featureProfilesService,
            IDbConnectionValidator dbConnectionValidator,
            IStringLocalizer<TenantValidator> stringLocalizer)
        {
            _shellHost = shellHost;
            _shellSettingsManager = shellSettingsManager;
            _featureProfilesService = featureProfilesService;
            _dbConnectionValidator = dbConnectionValidator;
            S = stringLocalizer;
        }

        public async Task<IEnumerable<ModelError>> ValidateAsync(TenantModelBase model)
        {
            var errors = new List<ModelError>();

            if (String.IsNullOrWhiteSpace(model.Name))
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

            if (!String.IsNullOrEmpty(model.Name) && !Regex.IsMatch(model.Name, @"^\w+$"))
            {
                errors.Add(new ModelError(nameof(model.Name), S["Invalid tenant name. Must contain characters only and no spaces."]));
            }

            _ = _shellHost.TryGetSettings(model.Name, out var existingShellSettings);

            if (!String.IsNullOrWhiteSpace(model.RequestUrlPrefix) && model.RequestUrlPrefix.Contains('/'))
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
                    shellSettings = _shellSettingsManager.CreateDefaultSettings();
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
            else if (existingShellSettings.IsUninitialized())
            {
                // Database settings may still have been changed.
                shellSettings = existingShellSettings;
            }

            if (shellSettings is not null)
            {
                var validationContext = new DbConnectionValidatorContext(shellSettings, model);
                await ValidateConnectionAsync(validationContext, errors);
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

                case DbConnectionValidatorResult.DocumentTableFound:
                    if (validationContext.DatabaseProvider == DatabaseProviderValue.Sqlite)
                    {
                        errors.Add(new ModelError(
                            String.Empty,
                            S["The related database file is already in use."]));
                        break;
                    }

                    errors.Add(new ModelError(
                        nameof(TenantViewModel.TablePrefix),
                        S["The provided database, table prefix and schema are already in use."]));
                    break;
            }
        }
    }
}

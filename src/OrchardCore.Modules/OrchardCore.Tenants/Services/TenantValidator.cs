using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Services
{
    public class TenantValidator : ITenantValidator
    {
        private readonly static char[] HostsSeparator = new char[] { ',' };

        private readonly IShellHost _shellHost;
        private readonly IFeatureProfilesService _featureProfilesService;
        private readonly IStringLocalizer<TenantValidator> S;
        private readonly IDbConnectionValidator _dbConnectionValidator;

        public TenantValidator(
            IShellHost shellHost,
            IFeatureProfilesService featureProfilesService,
            IDbConnectionValidator dbConnectionValidator,
            IStringLocalizer<TenantValidator> stringLocalizer)
        {
            _shellHost = shellHost;
            _featureProfilesService = featureProfilesService;
            _dbConnectionValidator = dbConnectionValidator;
            S = stringLocalizer;
        }

        public async Task<IEnumerable<ModelError>> ValidateAsync(TenantViewModel model)
        {
            var errors = new List<ModelError>();

            if (String.IsNullOrWhiteSpace(model.Name))
            {
                errors.Add(new ModelError(nameof(model.Name), S["The tenant name is mandatory."]));
            }

            if (!String.IsNullOrWhiteSpace(model.FeatureProfile))
            {
                var featureProfiles = await _featureProfilesService.GetFeatureProfilesAsync();
                if (!featureProfiles.ContainsKey(model.FeatureProfile))
                {
                    errors.Add(new ModelError(nameof(model.FeatureProfile), S["The feature profile does not exist."]));
                }
            }

            if (!String.IsNullOrEmpty(model.Name) && !Regex.IsMatch(model.Name, @"^\w+$"))
            {
                errors.Add(new ModelError(nameof(model.Name), S["Invalid tenant name. Must contain characters only and no spaces."]));
            }

            _shellHost.TryGetSettings(model.Name, out var shellSettings);

            if ((shellSettings == null || !shellSettings.IsDefaultShell()) &&
                String.IsNullOrWhiteSpace(model.RequestUrlHost) &&
                String.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                errors.Add(new ModelError(nameof(model.RequestUrlPrefix), S["Host and url prefix can not be empty at the same time."]));
            }

            if (!String.IsNullOrWhiteSpace(model.RequestUrlPrefix) && model.RequestUrlPrefix.Contains('/'))
            {
                errors.Add(new ModelError(nameof(model.RequestUrlPrefix), S["The url prefix can not contain more than one segment."]));
            }

            var allOtherSettings = _shellHost.GetAllSettings().Where(settings => !String.Equals(settings.Name, model.Name, StringComparison.OrdinalIgnoreCase));

            if (allOtherSettings.Any(settings => String.Equals(settings.RequestUrlPrefix, model.RequestUrlPrefix?.Trim(), StringComparison.OrdinalIgnoreCase) && DoesUrlHostExist(settings.RequestUrlHost, model.RequestUrlHost)))
            {
                errors.Add(new ModelError(nameof(model.RequestUrlPrefix), S["A tenant with the same host and prefix already exists."]));
            }

            if (model.IsNewTenant)
            {
                if (shellSettings != null)
                {
                    if (shellSettings.IsDefaultShell())
                    {
                        errors.Add(new ModelError(nameof(model.Name), S["The tenant name is in conflict with the 'Default' tenant."]));
                    }
                    else
                    {
                        errors.Add(new ModelError(nameof(model.Name), S["A tenant with the same name already exists."]));
                    }
                }

                await AssertConnectionValidityAndApplyErrorsAsync(model.DatabaseProvider, model.ConnectionString, model.TablePrefix, errors, model.Name);
            }
            else
            {
                if (shellSettings == null || shellSettings.State == TenantState.Uninitialized)
                {
                    // While the tenant is in Uninitialized state, we still are able to change the database settings.
                    // Let's validate the database for assurance.

                    await AssertConnectionValidityAndApplyErrorsAsync(model.DatabaseProvider, model.ConnectionString, model.TablePrefix, errors, model.Name);
                }
            }

            return errors;
        }

        private async Task AssertConnectionValidityAndApplyErrorsAsync(string databaseProvider, string connectionString, string tablePrefix, List<ModelError> errors, string shellName)
        {
            switch (await _dbConnectionValidator.ValidateAsync(databaseProvider, connectionString, tablePrefix, shellName))
            {
                case DbConnectionValidatorResult.UnsupportedProvider:
                    errors.Add(new ModelError(nameof(TenantViewModel.DatabaseProvider), S["The provided database provider is not supported."]));
                    break;
                case DbConnectionValidatorResult.InvalidConnection:
                    errors.Add(new ModelError(nameof(TenantViewModel.ConnectionString), S["The provided connection string is invalid or server is unreachable."]));
                    break;
                case DbConnectionValidatorResult.DocumentTableFound:
                    if (databaseProvider == DatabaseProviderValue.Sqlite)
                    {
                        errors.Add(new ModelError(String.Empty, S["The related database file is already in use."]));
                    }
                    else
                    {
                        errors.Add(new ModelError(nameof(TenantViewModel.TablePrefix), S["The provided database and table prefix are already in use."]));
                    }

                    break;
            }
        }

        private static bool DoesUrlHostExist(string urlHost, string modelUrlHost)
        {
            if (String.IsNullOrEmpty(urlHost) && String.IsNullOrEmpty(modelUrlHost))
            {
                return true;
            }

            var urlHosts = GetUrlHosts(urlHost);
            var modelUrlHosts = GetUrlHosts(modelUrlHost);

            return urlHosts.Intersect(modelUrlHosts).Any();
        }

        private static IEnumerable<string> GetUrlHosts(string combinedUrlHosts)
        {
            if (String.IsNullOrEmpty(combinedUrlHosts))
            {
                return Enumerable.Empty<string>();
            }

            return combinedUrlHosts.Split(HostsSeparator).Select(h => h.Trim());
        }
    }
}

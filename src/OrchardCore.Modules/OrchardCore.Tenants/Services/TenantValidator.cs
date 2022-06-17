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
        private readonly ShellSettings _shellSettings;
        private readonly IStringLocalizer<TenantValidator> S;
        private readonly IDbConnectionValidator _dbConnectionValidator;

        public TenantValidator(
            IShellHost shellHost,
            IFeatureProfilesService featureProfilesService,
            ShellSettings shellSettings,
            IStringLocalizer<TenantValidator> stringLocalizer,
            IDbConnectionValidator dbConnectionValidator)
        {
            _shellHost = shellHost;
            _featureProfilesService = featureProfilesService;
            _shellSettings = shellSettings;
            S = stringLocalizer;
            _dbConnectionValidator = dbConnectionValidator;
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

            if (!_shellSettings.IsDefaultShell() && String.IsNullOrWhiteSpace(model.RequestUrlHost) && String.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                errors.Add(new ModelError(nameof(model.RequestUrlPrefix), S["Host and url prefix can not be empty at the same time."]));
            }

            if (!String.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                if (model.RequestUrlPrefix.Contains('/'))
                {
                    errors.Add(new ModelError(nameof(model.RequestUrlPrefix), S["The url prefix can not contain more than one segment."]));
                }
            }

            var allSettings = _shellHost.GetAllSettings();

            var allOtherShells = allSettings.Where(t => !String.Equals(t.Name, model.Name, StringComparison.OrdinalIgnoreCase));

            if (allOtherShells.Any(tenant => String.Equals(tenant.RequestUrlPrefix, model.RequestUrlPrefix?.Trim(), StringComparison.OrdinalIgnoreCase) && DoesUrlHostExist(tenant.RequestUrlHost, model.RequestUrlHost)))
            {
                errors.Add(new ModelError(nameof(model.RequestUrlPrefix), S["A tenant with the same host and prefix already exists."]));
            }

            if (model.IsNewTenant)
            {
                if (allSettings.Any(tenant => String.Equals(tenant.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add(new ModelError(nameof(model.Name), S["A tenant with the same name already exists."]));
                }

                await AssertConnectionValidityAndApplyErrorsAsync(model.DatabaseProvider, model.ConnectionString, model.TablePrefix, errors);
            }
            else
            {
                // At this point, we know we are validating existing tenant
                var shellSetting = allSettings.Where(x => String.Equals(x.Name, model.Name, StringComparison.OrdinalIgnoreCase))
                                              .FirstOrDefault();

                if (shellSetting == null || shellSetting.State == TenantState.Uninitialized)
                {
                    // while the tenant is Uninitialized, we are still able to change the database settings
                    // let's validate the database for assurance

                    await AssertConnectionValidityAndApplyErrorsAsync(model.DatabaseProvider, model.ConnectionString, model.TablePrefix, errors);
                }
            }

            return errors;
        }

        private async Task AssertConnectionValidityAndApplyErrorsAsync(string databaseProvider, string connectionString, string tablePrefix, List<ModelError> errors)
        {
            switch (await _dbConnectionValidator.ValidateAsync(databaseProvider, connectionString, tablePrefix))
            {
                case DbConnectionValidatorResult.UnsupportedProvider:
                    errors.Add(new ModelError(nameof(TenantViewModel.DatabaseProvider), S["The provided database provider is not supported."]));
                    break;
                case DbConnectionValidatorResult.InvalidConnection:
                    errors.Add(new ModelError(nameof(TenantViewModel.ConnectionString), S["The provided connection string is invalid or unreachable."]));
                    break;
                case DbConnectionValidatorResult.DocumentFound:
                    errors.Add(new ModelError(nameof(TenantViewModel.TablePrefix), S["The provided table prefix already exists."]));
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

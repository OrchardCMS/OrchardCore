using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Services
{
    public class TenantValidator : ITenantValidator
    {
        private readonly static char[] HostsSeparator = new char[] { ',' };

        private readonly IShellHost _shellHost;
        private readonly IFeatureProfilesService _featureProfilesService;
        private readonly IEnumerable<DatabaseProvider> _databaseProviders;
        private readonly ShellSettings _shellSettings;
        private readonly IStringLocalizer<TenantValidator> S;
        private readonly IConnectionValidator _connectionValidator;

        public TenantValidator(
            IShellHost shellHost,
            IFeatureProfilesService featureProfilesService,
            IEnumerable<DatabaseProvider> databaseProviders,
            ShellSettings shellSettings,
            IStringLocalizer<TenantValidator> stringLocalizer,
            IConnectionValidator connectionValidator)
        {
            _shellHost = shellHost;
            _featureProfilesService = featureProfilesService;
            _databaseProviders = databaseProviders;
            _shellSettings = shellSettings;
            S = stringLocalizer;
            _connectionValidator = connectionValidator;
        }

        public async Task<IEnumerable<ModelError>> ValidateAsync(TenantViewModel model)
        {
            var errors = new List<ModelError>();
            var selectedProvider = _databaseProviders.FirstOrDefault(x => x.Value == model.DatabaseProvider);
            var hasConnectionString = !String.IsNullOrWhiteSpace(model.ConnectionString);

            if (selectedProvider != null && selectedProvider.HasConnectionString && !hasConnectionString)
            {
                errors.Add(new ModelError(nameof(model.ConnectionString), S["The connection string is mandatory for this provider."]));
            }

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

            if (model.IsNewTenant)
            {
                if (allSettings.Any(tenant => String.Equals(tenant.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add(new ModelError(nameof(model.Name), S["A tenant with the same name already exists."]));
                }

                if (selectedProvider != null && hasConnectionString)
                {
                    var result = await _connectionValidator.ValidateAsync(selectedProvider.Value, model.ConnectionString, model.TablePrefix);

                    if (result == ConnectionValidatorResult.InvalidConnection)
                    {
                        errors.Add(new ModelError(nameof(model.ConnectionString), S["The provided connection string is invalid or unreachable."]));
                    }
                    else if (result == ConnectionValidatorResult.ValidDocumentExists)
                    {
                        errors.Add(new ModelError(nameof(model.TablePrefix), S["The provided table prefix already exists."]));
                    }
                }
            }

            var allOtherShells = allSettings.Where(t => !String.Equals(t.Name, model.Name, StringComparison.OrdinalIgnoreCase));

            if (allOtherShells.Any(tenant => String.Equals(tenant.RequestUrlPrefix, model.RequestUrlPrefix?.Trim(), StringComparison.OrdinalIgnoreCase) && DoesUrlHostExist(tenant.RequestUrlHost, model.RequestUrlHost)))
            {
                errors.Add(new ModelError(nameof(model.RequestUrlPrefix), S["A tenant with the same host and prefix already exists."]));
            }

            return errors;
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

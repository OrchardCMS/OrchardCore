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
    public class TenantValidator
    {
        private readonly IShellHost _shellHost;
        private readonly IFeatureProfilesService _featureProfilesService;
        private readonly IEnumerable<DatabaseProvider> _databaseProviders;
        private readonly ShellSettings _shellSettings;
        private readonly IStringLocalizer<TenantValidator> S;

        public TenantValidator(
            IShellHost shellHost,
            IFeatureProfilesService featureProfilesService,
            IEnumerable<DatabaseProvider> databaseProviders,
            ShellSettings shellSettings,
            IStringLocalizer<TenantValidator> stringLocalizer)
        {
            _shellHost = shellHost;
            _featureProfilesService = featureProfilesService;
            _databaseProviders = databaseProviders;
            _shellSettings = shellSettings;
            S = stringLocalizer;
        }
        public async Task<IEnumerable<ModelError>> ValidateAsync(TenantViewModel model)
        {
            var errors = new List<ModelError>();
            var selectedProvider = _databaseProviders.FirstOrDefault(x => x.Value == model.DatabaseProvider);

            if (selectedProvider != null && selectedProvider.HasConnectionString && String.IsNullOrWhiteSpace(model.ConnectionString))
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

            var allSettings = _shellHost.GetAllSettings();

            if (model.IsNewTenant && allSettings.Any(tenant => String.Equals(tenant.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(new ModelError(nameof(model.Name), S["A tenant with the same name already exists."]));
            }

            if (!String.IsNullOrEmpty(model.Name) && !Regex.IsMatch(model.Name, @"^\w+$"))
            {
                errors.Add(new ModelError(nameof(model.Name), S["Invalid tenant name. Must contain characters only and no spaces."]));
            }

            if (!IsDefaultShell() && String.IsNullOrWhiteSpace(model.RequestUrlHost) && String.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                errors.Add(new ModelError(model.RequestUrlPrefix, S["Host and url prefix can not be empty at the same time."]));
            }

            var allOtherShells = allSettings.Where(t => !String.Equals(t.Name, model.Name, StringComparison.OrdinalIgnoreCase));
            if (allOtherShells.Any(t => (model.RequestUrlPrefix?.Trim()?.Equals(t.RequestUrlPrefix, StringComparison.OrdinalIgnoreCase) ?? false) && (t.RequestUrlHost?.Contains(model.RequestUrlHost, StringComparison.OrdinalIgnoreCase) ?? false)))
            {
                errors.Add(new ModelError(nameof(model.RequestUrlPrefix), S["A tenant with the same host and prefix already exists.", model.Name]));
            }

            if (!String.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                if (model.RequestUrlPrefix.Contains('/'))
                {
                    errors.Add(new ModelError(S["The url prefix can not contain more than one segment."], nameof(model.RequestUrlPrefix)));
                }
            }

            return errors;

            bool IsDefaultShell()
                => String.Equals(_shellSettings.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase);
        }
    }
}

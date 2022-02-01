using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tenants.ViewModels
{
    public class TenantViewModel : IValidatableObject
    {
        public string Description { get; set; }

        [Required]
        public string Name { get; set; }

        public string DatabaseProvider { get; set; }

        public string RequestUrlPrefix { get; set; }

        public string RequestUrlHost { get; set; }

        public string ConnectionString { get; set; }

        public string TablePrefix { get; set; }

        public string RecipeName { get; set; }

        public string FeatureProfile { get; set; }

        [BindNever]
        public bool IsNewTenant { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var shellHost = validationContext.GetService<IShellHost>();
            var featureProfilesService = validationContext.GetService<IFeatureProfilesService>();
            var databaseProviders = validationContext.GetServices<DatabaseProvider>();
            var S = validationContext.GetService<IStringLocalizer<TenantViewModel>>();

            var selectedProvider = databaseProviders.FirstOrDefault(x => x.Value == DatabaseProvider);

            if (selectedProvider != null && selectedProvider.HasConnectionString && String.IsNullOrWhiteSpace(ConnectionString))
            {
                yield return new ValidationResult(S["The connection string is mandatory for this provider."], new[] { nameof(ConnectionString) });
            }

            if (String.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult(S["The tenant name is mandatory."], new[] { nameof(Name) });
            }

            if (!String.IsNullOrWhiteSpace(FeatureProfile))
            {
                var featureProfiles = featureProfilesService.GetFeatureProfilesAsync().Result;
                if (!featureProfiles.ContainsKey(FeatureProfile))
                {
                    yield return new ValidationResult(S["The feature profile does not exist."], new[] { nameof(FeatureProfile) });
                }
            }

            var allSettings = shellHost.GetAllSettings();

            if (IsNewTenant && allSettings.Any(tenant => String.Equals(tenant.Name, Name, StringComparison.OrdinalIgnoreCase)))
            {
                yield return new ValidationResult(S["A tenant with the same name already exists."], new[] { nameof(Name) });
            }

            if (!String.IsNullOrEmpty(Name) && !Regex.IsMatch(Name, @"^\w+$"))
            {
                yield return new ValidationResult(S["Invalid tenant name. Must contain characters only and no spaces."], new[] { nameof(Name) });
            }

            if (!IsDefaultShell() && String.IsNullOrWhiteSpace(RequestUrlHost) && String.IsNullOrWhiteSpace(RequestUrlPrefix))
            {
                yield return new ValidationResult(S["Host and url prefix can not be empty at the same time."], new[] { nameof(RequestUrlPrefix) });
            }

            var allOtherShells = allSettings.Where(t => !String.Equals(t.Name, Name, StringComparison.OrdinalIgnoreCase));
            if (allOtherShells.Any(t => (RequestUrlPrefix?.Trim()?.Equals(t.RequestUrlPrefix, StringComparison.OrdinalIgnoreCase) ?? false) && (t.RequestUrlHost?.Contains(RequestUrlHost, StringComparison.OrdinalIgnoreCase) ?? false)))
            {
                yield return new ValidationResult(S["A tenant with the same host and prefix already exists.", Name], new[] { nameof(RequestUrlPrefix) });
            }

            if (!String.IsNullOrWhiteSpace(RequestUrlPrefix))
            {
                if (RequestUrlPrefix.Contains('/'))
                {
                    yield return new ValidationResult(S["The url prefix can not contain more than one segment."], new[] { nameof(RequestUrlPrefix) });
                }
            }

            bool IsDefaultShell()
            {
                var currentShellSettings = validationContext.GetService<ShellSettings>();

                return String.Equals(currentShellSettings.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

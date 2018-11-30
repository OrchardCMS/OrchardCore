using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Services
{
    public class AzureADService : IAzureADService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<AzureADService> T;
        private readonly ShellSettings _shellSettings;

        public AzureADService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<AzureADService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<AzureADSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<AzureADSettings>();
        }

        public async Task UpdateSettingsAsync(AzureADSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.GetSiteSettingsAsync();
            container.Properties[nameof(AzureADSettings)] = JObject.FromObject(settings);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public IEnumerable<ValidationResult> ValidateSettings(AzureADSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (string.IsNullOrWhiteSpace(settings.AppId))
            {
                yield return new ValidationResult("AppId is required", new string[] { nameof(settings.AppId) });
            }

            if (settings.Instance == null || !settings.Instance.IsAbsoluteUri)
            {
                yield return new ValidationResult("A valid absolute uri is required", new string[] { nameof(settings.Instance) });
            }

            if (String.IsNullOrWhiteSpace(settings.Domain))
            {
                yield return new ValidationResult("Please setup the domain of your AzureAD tenant", new string[] { nameof(settings.Instance) });
            }

            if (string.IsNullOrWhiteSpace(settings.TenantId))
            {
                yield return new ValidationResult("TenantId is required", new string[] { nameof(settings.TenantId) });
            }
        }
    }
}

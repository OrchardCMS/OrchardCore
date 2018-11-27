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
    public class MicrosoftAuthenticationService : IMicrosoftAuthenticationService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<MicrosoftAuthenticationService> T;
        private readonly ShellSettings _shellSettings;

        public MicrosoftAuthenticationService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<MicrosoftAuthenticationService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<MicrosoftAuthenticationSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<MicrosoftAuthenticationSettings>();
        }

        public async Task UpdateSettingsAsync(MicrosoftAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            var container = await _siteService.GetSiteSettingsAsync();
            container.Properties[nameof(MicrosoftAuthenticationSettings)] = JObject.FromObject(settings);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(MicrosoftAuthenticationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var results = ImmutableArray.CreateBuilder<ValidationResult>();
            return Task.FromResult<IEnumerable<ValidationResult>>(results);
        }
    }
}

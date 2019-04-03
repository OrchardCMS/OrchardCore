using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Services
{
    public class FacebookCoreService : IFacebookCoreService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<FacebookCoreService> T;
        private readonly ShellSettings _shellSettings;

        public FacebookCoreService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<FacebookCoreService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<FacebookCoreSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<FacebookCoreSettings>();
        }

        public async Task UpdateSettingsAsync(FacebookCoreSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.GetSiteSettingsAsync();
            container.Properties[nameof(FacebookCoreSettings)] = JObject.FromObject(settings);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(FacebookCoreSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var results = new List<ValidationResult>();

            if (string.IsNullOrEmpty(settings.AppId))
            {
                results.Add(new ValidationResult(T["The AppId is required."], new[]
                {
                    nameof(settings.AppId)
                }));
            }

            if (string.IsNullOrEmpty(settings.AppSecret))
            {
                results.Add(new ValidationResult(T["The App Secret is required."], new[]
                {
                    nameof(settings.AppSecret)
                }));
            }

            return Task.FromResult<IEnumerable<ValidationResult>>(results);
        }
    }
}

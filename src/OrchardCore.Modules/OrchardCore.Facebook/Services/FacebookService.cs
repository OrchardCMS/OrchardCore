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
    public class FacebookService : IFacebookService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<FacebookService> T;
        private readonly ShellSettings _shellSettings;

        public FacebookService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<FacebookService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<FacebookSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<FacebookSettings>();
        }

        public async Task UpdateSettingsAsync(FacebookSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.GetSiteSettingsAsync();
            container.Properties[nameof(FacebookSettings)] = JObject.FromObject(settings);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(FacebookSettings settings)
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

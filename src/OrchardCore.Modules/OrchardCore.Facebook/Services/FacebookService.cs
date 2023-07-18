using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly ISiteService _siteService;
        protected readonly IStringLocalizer S;

        public FacebookService(
            ISiteService siteService,
            IStringLocalizer<FacebookService> stringLocalizer)
        {
            _siteService = siteService;
            S = stringLocalizer;
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

            var container = await _siteService.LoadSiteSettingsAsync();
            container.Properties[nameof(FacebookSettings)] = JObject.FromObject(settings);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public IEnumerable<ValidationResult> ValidateSettings(FacebookSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var results = new List<ValidationResult>();

            if (String.IsNullOrEmpty(settings.AppId))
            {
                results.Add(new ValidationResult(S["The AppId is required."], new[]
                {
                    nameof(settings.AppId),
                }));
            }

            if (String.IsNullOrEmpty(settings.AppSecret))
            {
                results.Add(new ValidationResult(S["The App Secret is required."], new[]
                {
                    nameof(settings.AppSecret),
                }));
            }

            return results;
        }
    }
}

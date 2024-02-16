using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Facebook.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly ISiteService _siteService;
        protected readonly IStringLocalizer S;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public FacebookService(
            ISiteService siteService,
            IStringLocalizer<FacebookService> stringLocalizer,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _siteService = siteService;
            S = stringLocalizer;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task<FacebookSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<FacebookSettings>();
        }

        public async Task UpdateSettingsAsync(FacebookSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var container = await _siteService.LoadSiteSettingsAsync();
            container.Properties[nameof(FacebookSettings)] = JObject.FromObject(settings, _jsonSerializerOptions);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public IEnumerable<ValidationResult> ValidateSettings(FacebookSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var results = new List<ValidationResult>();

            if (string.IsNullOrEmpty(settings.AppId))
            {
                results.Add(new ValidationResult(S["The AppId is required."], new[]
                {
                    nameof(settings.AppId),
                }));
            }

            if (string.IsNullOrEmpty(settings.AppSecret))
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

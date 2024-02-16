using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Login.Services
{
    public class FacebookLoginService : IFacebookLoginService
    {
        private readonly ISiteService _siteService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public FacebookLoginService(
            ISiteService siteService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _siteService = siteService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task<FacebookLoginSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<FacebookLoginSettings>();
        }

        public async Task<FacebookLoginSettings> LoadSettingsAsync()
        {
            var container = await _siteService.LoadSiteSettingsAsync();
            return container.As<FacebookLoginSettings>();
        }

        public async Task UpdateSettingsAsync(FacebookLoginSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var container = await _siteService.LoadSiteSettingsAsync();
            container.Properties[nameof(FacebookLoginSettings)] = JObject.FromObject(settings, _jsonSerializerOptions);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(FacebookLoginSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            var results = ImmutableArray.CreateBuilder<ValidationResult>();
            return Task.FromResult<IEnumerable<ValidationResult>>(results);
        }
    }
}

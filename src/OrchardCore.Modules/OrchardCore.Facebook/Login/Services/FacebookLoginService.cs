using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Login.Services
{
    public class FacebookLoginService : IFacebookLoginService
    {
        private readonly ISiteService _siteService;

        public FacebookLoginService(
            ISiteService siteService)
        {
            _siteService = siteService;
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
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.LoadSiteSettingsAsync();
            container.Properties[nameof(FacebookLoginSettings)] = JObject.FromObject(settings);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public Task<IEnumerable<ValidationResult>> ValidateSettingsAsync(FacebookLoginSettings settings)
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

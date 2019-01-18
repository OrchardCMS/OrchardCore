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
    public class FacebookLoginService : IFacebookLoginService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<FacebookLoginService> T;
        private readonly ShellSettings _shellSettings;

        public FacebookLoginService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<FacebookLoginService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<FacebookLoginSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<FacebookLoginSettings>();
        }

        public async Task UpdateSettingsAsync(FacebookLoginSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            var container = await _siteService.GetSiteSettingsAsync();
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

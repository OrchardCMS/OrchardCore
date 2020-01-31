using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Twitter.Settings;
using OrchardCore.Settings;
using OrchardCore.Twitter.Signin.Settings;

namespace OrchardCore.Twitter.Signin.Services
{
    public class TwitterSigninService : ITwitterSigninService
    {
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<TwitterSigninService> T;
        private readonly ShellSettings _shellSettings;

        public TwitterSigninService(
            ISiteService siteService,
            ShellSettings shellSettings,
            IStringLocalizer<TwitterSigninService> stringLocalizer)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<TwitterSigninSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<TwitterSigninSettings>();
        }

        public async Task UpdateSettingsAsync(TwitterSigninSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            var container = await _siteService.LoadSiteSettingsAsync();
            container.Alter<TwitterSigninSettings>(nameof(TwitterSigninSettings), aspect =>
            {
                aspect.CallbackPath = settings.CallbackPath;
            });
            await _siteService.UpdateSiteSettingsAsync(container);
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Distributed.Redis.Settings;
using OrchardCore.Distributed.Redis.ViewModels;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using StackExchange.Redis;

namespace OrchardCore.Distributed.Redis.Drivers
{
    public class RedisSiteSettingsDisplayDriver : SectionDisplayDriver<ISite, RedisSettings>
    {
        public const string GroupId = "redis";
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public RedisSiteSettingsDisplayDriver(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(RedisSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, RedisPermissions.ManageRedisServices))
            {
                return null;
            }

            return Initialize<RedisSettingsViewModel>("RedisSettings_Edit", model =>
                {
                    model.Configuration = section.Configuration;
                }).Location("Content:2").OnGroup("redis");
        }

        public override async Task<IDisplayResult> UpdateAsync(RedisSettings section,  BuildEditorContext context)
        {
            if (context.GroupId == "redis")
            {
                var model = new RedisSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                try
                {
                   ConfigurationOptions.Parse(model.Configuration);
                }

                catch (Exception e)
                {
                    context.Updater.ModelState.AddModelError(nameof(model.Configuration),
                        "The configuration string is not valid: " + e.Message);
                }

                if (context.Updater.ModelState.IsValid)
                {
                    section.Configuration = model.Configuration;

                    // Reload the tenant to apply the settings
                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(section, context);
        }
    }
}

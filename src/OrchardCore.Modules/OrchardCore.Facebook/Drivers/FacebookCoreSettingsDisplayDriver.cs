using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Configuration;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;
using OrchardCore.Facebook.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers
{
    public class FacebookCoreSettingsDisplayDriver : SectionDisplayDriver<ISite, FacebookCoreSettings>
    {
        private const string SettingsGroupId = "OrchardCore.Facebook";

        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<FacebookCoreSettingsDisplayDriver> T;
        private readonly IFacebookCoreService _clientService;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public FacebookCoreSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IFacebookCoreService clientService,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IHtmlLocalizer<FacebookCoreSettingsDisplayDriver> stringLocalizer,
            IShellHost shellHost,
            ShellSettings shellSettings)
        {
            _authorizationService = authorizationService;
            _dataProtectionProvider = dataProtectionProvider;
            _clientService = clientService;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            T = stringLocalizer;
        }

        public override async Task<IDisplayResult> EditAsync(FacebookCoreSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
            {
                return null;
            }

            return Initialize<FacebookCoreSettingsViewModel>("FacebookCoreSettings_Edit", model =>
            {
                model.AppId = settings.AppId;
                model.AppSecret = settings.AppSecret;

            }).Location("Content:0").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(FacebookCoreSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
            {
                return null;
            }

            if (context.GroupId == SettingsGroupId)
            {
                var previousClientSecret = settings.AppSecret;
                var model = new FacebookCoreSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.AppId = model.AppId;

                // Restore the client secret if the input is empty (i.e if it hasn't been reset).
                if (string.IsNullOrEmpty(model.AppSecret))
                {
                    settings.AppSecret = previousClientSecret;
                }
                else
                {
                    var protector = _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core);
                    settings.AppSecret = protector.Protect(model.AppSecret);
                }

                foreach (var result in await _clientService.ValidateSettingsAsync(settings))
                {
                    if (result != ValidationResult.Success)
                    {
                        var key = result.MemberNames.FirstOrDefault() ?? string.Empty;
                        context.Updater.ModelState.AddModelError(key, result.ErrorMessage);
                    }
                }

                // If the settings are valid, reload the current tenant.
                if (context.Updater.ModelState.IsValid)
                {
                    _shellHost.ReloadShellContext(_shellSettings);
                }
            }

            return await EditAsync(settings, context);
        }
    }
}
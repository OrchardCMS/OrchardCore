using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;
using OrchardCore.Facebook.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers
{
    public class FacebookLoginSettingsDisplayDriver : SectionDisplayDriver<ISite, FacebookLoginSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<FacebookCoreSettingsDisplayDriver> T;
        private readonly IFacebookLoginService _clientService;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public FacebookLoginSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IFacebookLoginService clientService,
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

        public override async Task<IDisplayResult> EditAsync(FacebookLoginSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
            {
                return null;
            }

            return Initialize<FacebookLoginSettingsViewModel>("FacebookLoginSettings_Edit", model =>
            {
                model.CallbackPath = settings.CallbackPath;
            }).Location("Content:5").OnGroup(FacebookConstants.Features.Login);
        }

        public override async Task<IDisplayResult> UpdateAsync(FacebookLoginSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
            {
                return null;
            }

            if (context.GroupId == FacebookConstants.Features.Login)
            {

                var model = new FacebookLoginSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.CallbackPath = model.CallbackPath;


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
                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(settings, context);
        }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;
using OrchardCore.Facebook.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers
{
    public class FacebookSettingsDisplayDriver : SectionDisplayDriver<ISite, FacebookSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IFacebookService _clientService;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public FacebookSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IFacebookService clientService,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
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
        }

        public override async Task<IDisplayResult> EditAsync(FacebookSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
            {
                return null;
            }

            return Initialize<FacebookSettingsViewModel>("FacebookSettings_Edit", model =>
            {
                var protector = _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core);

                model.AppId = settings.AppId;
                model.FBInit = settings.FBInit;
                model.FBInitParams = settings.FBInitParams;
                model.Version = settings.Version;
                model.SdkJs = settings.SdkJs;
                if (!string.IsNullOrWhiteSpace(settings.AppSecret))
                {
                    model.AppSecret = protector.Unprotect(settings.AppSecret);
                }

            }).Location("Content:0").OnGroup(FacebookConstants.Features.Core);
        }

        public override async Task<IDisplayResult> UpdateAsync(FacebookSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == FacebookConstants.Features.Core)
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
                {
                    return null;
                }

                var model = new FacebookSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    var protector = _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core);
                    settings.AppId = model.AppId;
                    settings.AppSecret = protector.Protect(model.AppSecret);
                    settings.FBInit = model.FBInit;
                    settings.SdkJs = model.SdkJs;
                    if (!string.IsNullOrWhiteSpace(model.FBInitParams))
                        settings.FBInitParams = model.FBInitParams;
                    settings.Version = model.Version;

                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(settings, context);
        }
    }
}

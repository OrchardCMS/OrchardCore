using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Https.Settings;
using OrchardCore.Https.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Https.Drivers
{
    public class HttpsSettingsDisplayDriver : SectionDisplayDriver<ISite, HttpsSettings>
    {
        private const string SettingsGroupId = "Https";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        protected readonly IHtmlLocalizer H;

        public HttpsSettingsDisplayDriver(IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IHtmlLocalizer<HttpsSettingsDisplayDriver> htmlLocalizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _notifier = notifier;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            H = htmlLocalizer;
        }

        public override async Task<IDisplayResult> EditAsync(HttpsSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageHttps))
            {
                return null;
            }

            return Initialize<HttpsSettingsViewModel>("HttpsSettings_Edit", async model =>
            {
                var isHttpsRequest = _httpContextAccessor.HttpContext.Request.IsHttps;

                if (!isHttpsRequest)
                    await _notifier.WarningAsync(H["For safety, Enabling require HTTPS over HTTP has been prevented."]);

                model.EnableStrictTransportSecurity = settings.EnableStrictTransportSecurity;
                model.IsHttpsRequest = isHttpsRequest;
                model.RequireHttps = settings.RequireHttps;
                model.RequireHttpsPermanent = settings.RequireHttpsPermanent;
                model.SslPort = settings.SslPort ??
                                (isHttpsRequest && !settings.RequireHttps
                                    ? _httpContextAccessor.HttpContext.Request.Host.Port
                                    : null);
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(HttpsSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == SettingsGroupId)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageHttps))
                {
                    return null;
                }

                var model = new HttpsSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.EnableStrictTransportSecurity = model.EnableStrictTransportSecurity;
                settings.RequireHttps = model.RequireHttps;
                settings.RequireHttpsPermanent = model.RequireHttpsPermanent;
                settings.SslPort = model.SslPort;

                // If the settings are valid, release the current tenant.
                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(settings, context);
        }
    }
}

using System.Threading.Tasks;
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
        private const int DefaultSslPort = 443;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHtmlLocalizer T;

        public HttpsSettingsDisplayDriver(IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IHtmlLocalizer<HttpsSettingsDisplayDriver> stringLocalizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            T = stringLocalizer;
        }

        public override IDisplayResult Edit(HttpsSettings settings, BuildEditorContext context)
        {
            return Initialize<HttpsSettingsViewModel>("HttpsSettings_Edit", model =>
            {
                var isHttpsRequest = _httpContextAccessor.HttpContext.Request.IsHttps;

                if (!isHttpsRequest)
                    _notifier.Warning(T["For safety, Enabling require HTTPS over HTTP has been prevented."]);

                model.EnableStrictTransportSecurity = settings.EnableStrictTransportSecurity;
                model.IsHttpsRequest = isHttpsRequest;
                model.RequireHttps = settings.RequireHttps;
                model.RequireHttpsPermanent = settings.RequireHttpsPermanent;
                model.SslPort = settings.SslPort ??
                                (_httpContextAccessor.HttpContext.Request.Host.Port == DefaultSslPort
                                    ? null
                                    : _httpContextAccessor.HttpContext.Request.Host.Port);
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(HttpsSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == SettingsGroupId)
            {
                var model = new HttpsSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.EnableStrictTransportSecurity = model.EnableStrictTransportSecurity;
                settings.RequireHttps = model.RequireHttps;
                settings.RequireHttpsPermanent = model.RequireHttpsPermanent;
                settings.SslPort = model.SslPort;

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

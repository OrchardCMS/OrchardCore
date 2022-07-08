using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.ReverseProxy.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy.Drivers
{
    public class ReverseProxySettingsDisplayDriver : SectionDisplayDriver<ISite, ReverseProxySettings>
    {
        private const string SettingsGroupId = "ReverseProxy";
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public ReverseProxySettingsDisplayDriver(
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

        public override async Task<IDisplayResult> EditAsync(ReverseProxySettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReverseProxySettings))
            {
                return null;
            }

            return Initialize<ReverseProxySettingsViewModel>("ReverseProxySettings_Edit", model =>
            {
                model.EnableXForwardedFor = settings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor);
                model.EnableXForwardedHost = settings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost);
                model.EnableXForwardedProto = settings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto);
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ReverseProxySettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageReverseProxySettings))
            {
                return null;
            }

            if (context.GroupId == SettingsGroupId)
            {
                var model = new ReverseProxySettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                section.ForwardedHeaders = ForwardedHeaders.None;

                if (model.EnableXForwardedFor)
                    section.ForwardedHeaders |= ForwardedHeaders.XForwardedFor;

                if (model.EnableXForwardedHost)
                    section.ForwardedHeaders |= ForwardedHeaders.XForwardedHost;

                if (model.EnableXForwardedProto)
                    section.ForwardedHeaders |= ForwardedHeaders.XForwardedProto;

                // If the settings are valid, release the current tenant.
                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(section, context);
        }
    }
}

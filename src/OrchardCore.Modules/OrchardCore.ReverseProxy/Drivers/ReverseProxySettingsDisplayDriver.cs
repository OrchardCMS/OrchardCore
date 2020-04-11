using System.Threading.Tasks;
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

        public ReverseProxySettingsDisplayDriver(
            IShellHost shellHost,
            ShellSettings shellSettings)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override IDisplayResult Edit(ReverseProxySettings section, BuildEditorContext context)
        {
            return Initialize<ReverseProxySettingsViewModel>("ReverseProxySettings_Edit", model =>
            {
                model.EnableXForwardedFor = section.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor);
                model.EnableXForwardedHost = section.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost);
                model.EnableXForwardedProto = section.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto);
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ReverseProxySettings section, BuildEditorContext context)
        {
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

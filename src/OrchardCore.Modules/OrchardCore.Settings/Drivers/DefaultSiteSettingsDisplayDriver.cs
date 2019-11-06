using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Drivers
{
    public class DefaultSiteSettingsDisplayDriver : DisplayDriver<ISite>
    {
        public const string GroupId = "general";

        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public DefaultSiteSettingsDisplayDriver(
            IShellHost shellHost,
            ShellSettings shellSettings
            )
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public override IDisplayResult Edit(ISite site)
        {
            return Initialize<SiteSettingsViewModel>("Settings_Edit", model =>
            {
                model.SiteName = site.SiteName;
                model.PageTitleFormat = site.PageTitleFormat;
                model.BaseUrl = site.BaseUrl;
                model.TimeZone = site.TimeZoneId;
                model.PageSize = site.PageSize;
                model.UseCdn = site.UseCdn;
                model.CdnBaseUrl = site.CdnBaseUrl;
                model.ResourceDebugMode = site.ResourceDebugMode;
                model.AppendVersion = site.AppendVersion;
            }).Location("Content:1").OnGroup(GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                var model = new SiteSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix))
                {
                    site.SiteName = model.SiteName;
                    site.PageTitleFormat = model.PageTitleFormat;
                    site.BaseUrl = model.BaseUrl;
                    site.TimeZoneId = model.TimeZone;
                    site.PageSize = model.PageSize;
                    site.UseCdn = model.UseCdn;
                    site.CdnBaseUrl = model.CdnBaseUrl;
                    site.ResourceDebugMode = model.ResourceDebugMode;
                    site.AppendVersion = model.AppendVersion;

                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }

            return Edit(site);
        }
    }
}

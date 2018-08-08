using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Drivers
{
    public class DefaultSiteSettingsDisplayDriver : DisplayDriver<ISite>
    {
        public const string GroupId = "general";
        private readonly INotifier _notifier;

        public DefaultSiteSettingsDisplayDriver(
            INotifier notifier,
            IHtmlLocalizer<DefaultSiteSettingsDisplayDriver> h)
        {
            _notifier = notifier;
            H = h;
        }

        IHtmlLocalizer H { get; set; }

        public override Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                    Initialize<SiteSettingsViewModel>("Settings_Edit", model =>
                    {
                        model.SiteName = site.SiteName;
                        model.BaseUrl = site.BaseUrl;
                        model.TimeZone = site.TimeZoneId;
                        model.Culture = site.Culture;
                        model.SiteCultures = site.SupportedCultures.Select(x => CultureInfo.GetCultureInfo(x));
                    }).Location("Content:1").OnGroup(GroupId)
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                var previousCulture = site.Culture;

                var model = new SiteSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix, t => t.SiteName, t => t.BaseUrl, t => t.TimeZone, t => t.Culture))
                {
                    site.SiteName = model.SiteName;
                    site.BaseUrl = model.BaseUrl;
                    site.TimeZoneId = model.TimeZone;
                    site.Culture = model.Culture;
                }

                if (site.Culture != previousCulture)
                {
                    _notifier.Warning(H["The site needs to be restarted for the settings to take effect"]);
                }
            }

            return Edit(site);
        }
    }
}

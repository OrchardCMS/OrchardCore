using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization.Services;
using OrchardCore.Modules.Services;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Drivers
{
    public class DefaultSiteSettingsDisplayDriver : DisplayDriver<ISite>
    {
        public const string GroupId = "general";

        private readonly ICultureManager _cultureManager;
        private readonly ILocalCulture _localCulture;

        public DefaultSiteSettingsDisplayDriver(
            ICultureManager cultureManager,
            ILocalCulture localCulture)
        {
            _cultureManager = cultureManager;
            _localCulture = localCulture;
        }

        public override Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                    Initialize<SiteSettingsViewModel>("Settings_Edit", model =>
                    {
                        model.SiteName = site.SiteName;
                        model.BaseUrl = site.BaseUrl;
                        model.TimeZone = site.TimeZoneId;
                        model.Culture = site.Culture;
                        model.SiteCultures = _cultureManager.ListCultures()?.Select(x => CultureInfo.GetCultureInfo(x.CultureName));
                        model.LocalizationEnabled = _localCulture.IsLocalizationEnabled();
                    }).Location("Content:1").OnGroup(GroupId)
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
        {
            if (context.GroupId == GroupId)
            {
                var model = new SiteSettingsViewModel();

                if (await context.Updater.TryUpdateModelAsync(model, Prefix, t => t.SiteName, t => t.BaseUrl, t => t.TimeZone, t => t.Culture))
                {
                    site.SiteName = model.SiteName;
                    site.BaseUrl = model.BaseUrl;
                    site.TimeZoneId = model.TimeZone;
                    site.Culture = model.Culture;
                }
            }

            return Edit(site);
        }
    }
}

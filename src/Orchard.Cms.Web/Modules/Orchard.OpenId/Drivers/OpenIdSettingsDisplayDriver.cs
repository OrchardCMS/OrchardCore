using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Environment.Shell;
using Orchard.OpenId.Services;
using Orchard.OpenId.Settings;
using Orchard.OpenId.ViewModels;
using Orchard.Settings;
using Orchard.Settings.Services;
using System;
using System.Threading.Tasks;

namespace Orchard.OpenId.Drivers
{
    public class OpenIdSiteSettingsDisplayDriver : SiteSettingsSectionDisplayDriver<OpenIdSettings>
    {
        private readonly IOpenIdService _openIdServices;
        private readonly ISiteService _siteService;
        private readonly ShellSettings _shellSettings;

        public OpenIdSiteSettingsDisplayDriver(IOpenIdService openIdServices, 
                                                ISiteService siteService, 
                                                ShellSettings shellSettings)
        {
            _openIdServices = openIdServices;
            _siteService = siteService;
            _shellSettings = shellSettings;
        }

        public override IDisplayResult Edit(OpenIdSettings settings, BuildEditorContext context)
        {
            var sslBaseUrl = new Uri(_siteService.GetSiteSettingsAsync().Result.BaseUrl.Replace("http://", "https://") + _shellSettings.RequestUrlPrefix);
            
            return Shape<OpenIdSettingsViewModel>("OpenIdSettings_Edit", model =>
                {
                    model.TestingModeEnabled = settings.TestingModeEnabled;
                    model.DefaultTokenFormat = settings.DefaultTokenFormat;
                    model.Authority = settings.Authority;
                    model.Audience = settings.Audience;
                    model.CertificateStoreLocation = settings.CertificateStoreLocation;
                    model.CertificateStoreName = settings.CertificateStoreName;
                    model.CertificateThumbPrint = settings.CertificateThumbPrint;
                    model.AvailableCertificates = _openIdServices.GetAvailableCertificates();
                    model.SslBaseUrl = sslBaseUrl.AbsoluteUri.TrimEnd(new[] { '/' });
                }).Location("Content:2").OnGroup("open id");
        }

        public override async Task<IDisplayResult> UpdateAsync(OpenIdSettings settings, IUpdateModel updater)
        {
            var model = new OpenIdSettingsViewModel();

            await updater.TryUpdateModelAsync(model, Prefix);
            model.Authority = model.Authority ?? "".Trim();
            model.Audience = model.Audience ?? "".Trim();
            
            settings.TestingModeEnabled = model.TestingModeEnabled;
            settings.DefaultTokenFormat = model.DefaultTokenFormat;
            settings.Authority = model.Authority;
            settings.Audience = model.Audience;
            settings.CertificateStoreLocation = model.CertificateStoreLocation;
            settings.CertificateStoreName = model.CertificateStoreName;
            settings.CertificateThumbPrint = model.CertificateThumbPrint;

            _openIdServices.IsValidOpenIdSettings(settings, updater.ModelState);
            return Edit(settings);
        }
    }
}

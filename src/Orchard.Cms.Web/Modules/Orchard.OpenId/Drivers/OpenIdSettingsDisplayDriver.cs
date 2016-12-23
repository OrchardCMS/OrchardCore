using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.OpenId.Services;
using Orchard.OpenId.Settings;
using Orchard.OpenId.ViewModels;
using Orchard.Settings.Services;
using System;
using System.Threading.Tasks;

namespace Orchard.OpenId.Drivers
{
    public class OpenIdSiteSettingsDisplayDriver : SiteSettingsSectionDisplayDriver<OpenIdSettings>
    {
        private readonly IOpenIdService _openIdServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OpenIdSiteSettingsDisplayDriver(IOpenIdService openIdServices,
                                                IHttpContextAccessor httpContextAccessor)
        {
            _openIdServices = openIdServices;
            _httpContextAccessor = httpContextAccessor;
        }

        public override IDisplayResult Edit(OpenIdSettings settings, BuildEditorContext context)
        {
            var requestUrl = _httpContextAccessor.HttpContext.Request.GetDisplayUrl();
            return Shape<OpenIdSettingsViewModel>("OpenIdSettings_Edit", model =>
                {
                    model.TestingModeEnabled = settings.TestingModeEnabled;
                    model.AccessTokenFormat = settings.AccessTokenFormat;
                    model.Authority = settings.Authority;
                    model.Audiences = settings.Audiences != null ? string.Join(",", settings.Audiences) : null;
                    model.CertificateStoreLocation = settings.CertificateStoreLocation;
                    model.CertificateStoreName = settings.CertificateStoreName;
                    model.CertificateThumbPrint = settings.CertificateThumbPrint;
                    model.AvailableCertificates = _openIdServices.GetAvailableCertificates(onlyCertsWithPrivateKey: true);
                    model.SslBaseUrl = requestUrl.Remove(requestUrl.IndexOf("/Admin/Settings")).Replace("http://", "https://");
                    model.EnableTokenEndpoint = settings.EnableTokenEndpoint;
                    model.EnableAuthorizationEndpoint = settings.EnableAuthorizationEndpoint;
                    model.EnableLogoutEndpoint = settings.EnableLogoutEndpoint;
                    model.EnableUserInfoEndpoint = settings.EnableUserInfoEndpoint;
                    model.AllowPasswordFlow = settings.AllowPasswordFlow;
                    model.AllowClientCredentialsFlow = settings.AllowClientCredentialsFlow;
                    model.AllowAuthorizationCodeFlow = settings.AllowAuthorizationCodeFlow;
                    model.AllowRefreshTokenFlow = settings.AllowRefreshTokenFlow;
                    model.AllowImplicitFlow = settings.AllowImplicitFlow;
                    model.AllowHybridFlow = settings.AllowHybridFlow;
                }).Location("Content:2").OnGroup("open id");
        }

        public override async Task<IDisplayResult> UpdateAsync(OpenIdSettings settings, IUpdateModel updater, string groupId)
        {
            if (groupId == "open id")
            {
                var model = new OpenIdSettingsViewModel();

                await updater.TryUpdateModelAsync(model, Prefix);
                model.Authority = model.Authority ?? "".Trim();
                model.Audiences = model.Audiences ?? "".Trim();

                settings.TestingModeEnabled = model.TestingModeEnabled;
                settings.AccessTokenFormat = model.AccessTokenFormat;
                settings.Authority = model.Authority;
                settings.Audiences = model.Audiences.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                settings.CertificateStoreLocation = model.CertificateStoreLocation;
                settings.CertificateStoreName = model.CertificateStoreName;
                settings.CertificateThumbPrint = model.CertificateThumbPrint;
                settings.EnableTokenEndpoint = model.EnableTokenEndpoint;
                settings.EnableAuthorizationEndpoint = model.EnableAuthorizationEndpoint;
                settings.EnableLogoutEndpoint = model.EnableLogoutEndpoint;
                settings.EnableUserInfoEndpoint = model.EnableUserInfoEndpoint;
                settings.AllowPasswordFlow = model.AllowPasswordFlow;
                settings.AllowClientCredentialsFlow = model.AllowClientCredentialsFlow;
                settings.AllowAuthorizationCodeFlow = model.AllowAuthorizationCodeFlow;
                settings.AllowRefreshTokenFlow = model.AllowRefreshTokenFlow;
                settings.AllowImplicitFlow = model.AllowImplicitFlow;
                settings.AllowHybridFlow = model.AllowHybridFlow;

                _openIdServices.IsValidOpenIdSettings(settings, updater.ModelState);
            }

            return Edit(settings);
        }
    }
}

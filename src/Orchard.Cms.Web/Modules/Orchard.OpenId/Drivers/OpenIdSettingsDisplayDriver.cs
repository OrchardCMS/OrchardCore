using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Memory;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
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
        private const string RestartPendingCacheKey = "OpenIdSiteSettings_RestartPending";
        private const string SettingsGroupId = "open id";

        private readonly IOpenIdService _openIdServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<OpenIdSiteSettingsDisplayDriver> T;
        private readonly IMemoryCache _memoryCache;

        public OpenIdSiteSettingsDisplayDriver(IOpenIdService openIdServices,
                                                IHttpContextAccessor httpContextAccessor,
                                                INotifier notifier,
                                                IHtmlLocalizer<OpenIdSiteSettingsDisplayDriver> stringLocalizer,
                                                IMemoryCache memoryCache)
        {
            _openIdServices = openIdServices;
            _notifier = notifier;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            T = stringLocalizer;
        }

        public override IDisplayResult Edit(OpenIdSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == SettingsGroupId && _memoryCache.Get(RestartPendingCacheKey)!=null)
                _notifier.Warning(T["The site needs to be restarted for the settings to take effect"]);

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
            if (groupId == SettingsGroupId)
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

                if (_openIdServices.IsValidOpenIdSettings(settings, updater.ModelState) && _memoryCache.Get(RestartPendingCacheKey) == null)
                {
                    var entry = _memoryCache.CreateEntry(RestartPendingCacheKey);                    
                    _memoryCache.Set(entry.Key, entry, new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove });
                }
            }

            return Edit(settings);
        }
    }
}

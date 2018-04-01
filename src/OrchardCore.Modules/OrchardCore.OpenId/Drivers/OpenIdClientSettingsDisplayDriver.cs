using OrchardCore.OpenId.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using System;
using System.Threading.Tasks;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.Services;

namespace OrchardCore.OpenId.Drivers
{
    public class OpenIdConnectSettingsDisplayDriver : SectionDisplayDriver<ISite, OpenIdClientSettings>
    {
        private const string RestartPendingCacheKey = "OpenIdConnect_RestartPending";
        private const string SettingsGroupId = "OrchardCore.OpenId.Cient";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<OpenIdConnectSettingsDisplayDriver> T;
        private readonly IMemoryCache _memoryCache;
        private readonly IOpenIdClientService _openIdConnectService;


        public OpenIdConnectSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IHtmlLocalizer<OpenIdConnectSettingsDisplayDriver> stringLocalizer,
            IMemoryCache memoryCache,
            IOpenIdClientService openIdConnectService
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            T = stringLocalizer;
            _memoryCache = memoryCache;
            _openIdConnectService = openIdConnectService;
        }

        public override IDisplayResult Edit(OpenIdClientSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == SettingsGroupId && _memoryCache.Get(RestartPendingCacheKey) != null)
                _notifier.Warning(T["The site needs to be restarted for the settings to take effect"]);

            var requestUrl = _httpContextAccessor.HttpContext.Request.GetDisplayUrl();
            return Shape<OpenIdClientSettingsViewModel>("OpenIdClientSettings_Edit", model =>
            {
                model.DisplayName = settings.DisplayName;
                model.TestingModeEnabled = settings.TestingModeEnabled;
                model.AllowedScopes = settings.AllowedScopes != null ? string.Join(",", settings.AllowedScopes) : null;
                model.Authority = settings.Authority;
                model.CallbackPath = settings.CallbackPath;
                model.ClientId = settings.ClientId;
                model.ClientSecret = _openIdConnectService.Unprotect(settings.ClientSecret);
                model.SignedOutCallbackPath = settings.SignedOutCallbackPath;
                model.SignedOutRedirectUri = settings.SignedOutRedirectUri;
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(OpenIdClientSettings settings, IUpdateModel updater, string groupId)
        {
            if (groupId == SettingsGroupId)
            {
                var previousClientSecret = settings.ClientSecret;
                var model = new OpenIdClientSettingsViewModel();
                await updater.TryUpdateModelAsync(model, Prefix);
                settings.DisplayName = model.DisplayName;
                settings.TestingModeEnabled = model.TestingModeEnabled;
                settings.AllowedScopes = model.AllowedScopes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                settings.Authority = model.Authority;
                settings.CallbackPath = new PathString(model.CallbackPath);
                settings.ClientId = model.ClientId;
                settings.ClientSecret = _openIdConnectService.Protect(model.ClientSecret);
                settings.SignedOutCallbackPath = model.SignedOutCallbackPath;
                settings.SignedOutRedirectUri = model.SignedOutRedirectUri;

                if (_openIdConnectService.IsValidOpenIdConnectSettings(settings, updater.ModelState) && _memoryCache.Get(RestartPendingCacheKey) == null)
                {
                    var entry = _memoryCache.CreateEntry(RestartPendingCacheKey);
                    _memoryCache.Set(entry.Key, entry, new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove });
                }
            }
            return Edit(settings);
        }
    }
}
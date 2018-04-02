using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.OpenId.Configuration;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Drivers
{
    public class OpenIdClientSettingsDisplayDriver : SectionDisplayDriver<ISite, OpenIdClientSettings>
    {
        private const string RestartPendingCacheKey = "OpenIdConnect_RestartPending";
        private const string SettingsGroupId = "OrchardCore.OpenId.Client";

        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<OpenIdClientSettingsDisplayDriver> T;
        private readonly IMemoryCache _memoryCache;
        private readonly IOpenIdClientService _clientService;

        public OpenIdClientSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IOpenIdClientService clientService,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IHtmlLocalizer<OpenIdClientSettingsDisplayDriver> stringLocalizer,
            IMemoryCache memoryCache)
        {
            _authorizationService = authorizationService;
            _dataProtectionProvider = dataProtectionProvider;
            _clientService = clientService;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            T = stringLocalizer;
            _memoryCache = memoryCache;
        }

        public override async Task<IDisplayResult> EditAsync(OpenIdClientSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == SettingsGroupId && _memoryCache.Get(RestartPendingCacheKey) != null)
                _notifier.Warning(T["The site needs to be restarted for the settings to take effect"]);

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageClientSettings))
            {
                return null;
            }

            return Shape<OpenIdClientSettingsViewModel>("OpenIdClientSettings_Edit", model =>
            {
                model.DisplayName = settings.DisplayName;
                model.AllowedScopes = settings.AllowedScopes != null ? string.Join(",", settings.AllowedScopes) : null;
                model.Authority = settings.Authority;
                model.CallbackPath = settings.CallbackPath;
                model.ClientId = settings.ClientId;
                model.ClientSecret = settings.ClientSecret;
                model.SignedOutCallbackPath = settings.SignedOutCallbackPath;
                model.SignedOutRedirectUri = settings.SignedOutRedirectUri;
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(OpenIdClientSettings settings, IUpdateModel updater, string groupId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageClientSettings))
            {
                return null;
            }

            if (groupId == SettingsGroupId)
            {
                var previousClientSecret = settings.ClientSecret;
                var model = new OpenIdClientSettingsViewModel();
                await updater.TryUpdateModelAsync(model, Prefix);

                settings.DisplayName = model.DisplayName;
                settings.AllowedScopes = model.AllowedScopes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                settings.Authority = model.Authority;
                settings.CallbackPath = model.CallbackPath;
                settings.ClientId = model.ClientId;
                settings.SignedOutCallbackPath = model.SignedOutCallbackPath;
                settings.SignedOutRedirectUri = model.SignedOutRedirectUri;

                // Restore the client secret if the input is empty (i.e if it hasn't been reset).
                if (string.IsNullOrEmpty(settings.ClientSecret))
                {
                    settings.ClientSecret = previousClientSecret;
                }
                else
                {
                    var protector = _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration));
                    settings.ClientSecret = protector.Protect(settings.ClientSecret);
                }

                foreach (var result in await _clientService.ValidateSettingsAsync(settings))
                {
                    if (result != ValidationResult.Success)
                    {
                        var key = result.MemberNames.FirstOrDefault() ?? string.Empty;
                        updater.ModelState.AddModelError(key, result.ErrorMessage);
                    }
                }

                if (updater.ModelState.IsValid && _memoryCache.Get(RestartPendingCacheKey) == null)
                {
                    var entry = _memoryCache.CreateEntry(RestartPendingCacheKey);
                    _memoryCache.Set(entry.Key, entry, new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove });
                }
            }

            return Edit(settings);
        }
    }
}
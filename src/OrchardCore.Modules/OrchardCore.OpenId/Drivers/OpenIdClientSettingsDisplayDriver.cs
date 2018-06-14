using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Environment.Shell;
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
        private readonly ShellSettings _shellSettings;

        public OpenIdClientSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IOpenIdClientService clientService,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IHtmlLocalizer<OpenIdClientSettingsDisplayDriver> stringLocalizer,
            IMemoryCache memoryCache,
            ShellSettings shellSettings)
        {
            _authorizationService = authorizationService;
            _dataProtectionProvider = dataProtectionProvider;
            _clientService = clientService;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            T = stringLocalizer;
            _memoryCache = memoryCache;
            _shellSettings = shellSettings;
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

            return Initialize<OpenIdClientSettingsViewModel>("OpenIdClientSettings_Edit", model =>
            {
                model.DisplayName = settings.DisplayName;
                model.Scopes = settings.Scopes != null ? string.Join(" ", settings.Scopes) : null;
                model.Authority = settings.Authority;
                model.CallbackPath = settings.CallbackPath;
                model.ClientId = settings.ClientId;
                model.ClientSecret = settings.ClientSecret;
                model.SignedOutCallbackPath = settings.SignedOutCallbackPath;
                model.SignedOutRedirectUri = settings.SignedOutRedirectUri;
                model.ResponseMode = settings.ResponseMode;

                if (settings.ResponseType == OpenIdConnectResponseType.Code)
                {
                    model.UseCodeFlow = true;
                }
                else if (settings.ResponseType == OpenIdConnectResponseType.CodeIdToken)
                {
                    model.UseCodeIdTokenFlow = true;
                }
                else if (settings.ResponseType == OpenIdConnectResponseType.CodeIdTokenToken)
                {
                    model.UseCodeIdTokenTokenFlow = true;
                }
                else if (settings.ResponseType == OpenIdConnectResponseType.CodeToken)
                {
                    model.UseCodeTokenFlow = true;
                }
                else if (settings.ResponseType == OpenIdConnectResponseType.IdToken)
                {
                    model.UseIdTokenFlow = true;
                }
                else if (settings.ResponseType == OpenIdConnectResponseType.IdTokenToken)
                {
                    model.UseIdTokenTokenFlow = true;
                }


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

                model.Scopes = model.Scopes ?? string.Empty;

                settings.DisplayName = model.DisplayName;
                settings.Scopes = model.Scopes.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                settings.Authority = model.Authority;
                settings.CallbackPath = model.CallbackPath;
                settings.ClientId = model.ClientId;
                settings.SignedOutCallbackPath = model.SignedOutCallbackPath;
                settings.SignedOutRedirectUri = model.SignedOutRedirectUri;
                settings.ResponseMode = model.ResponseMode;

                bool useClientSecret = true;

                if (model.UseCodeFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.Code;

                }
                else if (model.UseCodeIdTokenFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                }
                else if (model.UseCodeIdTokenTokenFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.CodeIdTokenToken;
                }
                else if (model.UseCodeTokenFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.CodeToken;
                }
                else if (model.UseIdTokenFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.IdToken;
                    useClientSecret = false;
                }
                else if (model.UseIdTokenTokenFlow)
                {
                    settings.ResponseType = OpenIdConnectResponseType.IdTokenToken;
                    useClientSecret = false;
                }
                else
                {
                    settings.ResponseType = OpenIdConnectResponseType.None;
                    useClientSecret = false;
                }

                if (!useClientSecret)
                {
                    model.ClientSecret = previousClientSecret = null;
                }

                // Restore the client secret if the input is empty (i.e if it hasn't been reset).
                if (string.IsNullOrEmpty(model.ClientSecret))
                {
                    settings.ClientSecret = previousClientSecret;
                }
                else
                {
                    var protector = _dataProtectionProvider.CreateProtector(nameof(OpenIdClientConfiguration), _shellSettings.Name);
                    settings.ClientSecret = protector.Protect(model.ClientSecret);
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
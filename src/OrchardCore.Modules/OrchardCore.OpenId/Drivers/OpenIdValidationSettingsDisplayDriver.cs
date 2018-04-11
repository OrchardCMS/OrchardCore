using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Drivers
{
    public class OpenIdValidationSettingsDisplayDriver : SectionDisplayDriver<ISite, OpenIdValidationSettings>
    {
        private const string RestartPendingCacheKey = "OpenIdSiteSettings_RestartPending";
        private const string SettingsGroupId = "OrchardCore.OpenId.Validation";

        private readonly IAuthorizationService _authorizationService;
        private readonly IOpenIdValidationService _validationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<OpenIdValidationSettingsDisplayDriver> T;
        private readonly IMemoryCache _memoryCache;
        private readonly IShellHost _shellHost;
        private readonly IShellSettingsManager _shellSettingsManager;

        public OpenIdValidationSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IOpenIdValidationService validationService,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IHtmlLocalizer<OpenIdValidationSettingsDisplayDriver> stringLocalizer,
            IMemoryCache memoryCache,
            IShellHost shellHost,
            IShellSettingsManager shellSettingsManager)
        {
            _authorizationService = authorizationService;
            _validationService = validationService;
            _notifier = notifier;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _shellHost = shellHost;
            _shellSettingsManager = shellSettingsManager;
            T = stringLocalizer;
        }

        public override async Task<IDisplayResult> EditAsync(OpenIdValidationSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageValidationSettings))
            {
                return null;
            }

            if (context.GroupId == SettingsGroupId && _memoryCache.Get(RestartPendingCacheKey) != null)
                _notifier.Warning(T["The site needs to be restarted for the settings to take effect"]);

            return Shape<OpenIdValidationSettingsViewModel>("OpenIdValidationSettings_Edit", model =>
            {
                model.Authority = settings.Authority;
                model.Audience = settings.Audience;
                model.Tenant = settings.Tenant;

                model.AvailableTenants = (from tenant in _shellSettingsManager.LoadSettings().Where(s => s.State == TenantState.Running).AsParallel()
                                          let provider = _shellHost.GetOrCreateShellContext(tenant).ServiceProvider
                                          let descriptor = provider.GetRequiredService<ShellDescriptor>()
                                          where descriptor.Features.Any(feature => feature.Id == OpenIdConstants.Features.Server)
                                          select tenant.Name).ToList();
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(OpenIdValidationSettings settings, IUpdateModel updater, string groupId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageValidationSettings))
            {
                return null;
            }

            if (groupId == SettingsGroupId)
            {
                var model = new OpenIdValidationSettingsViewModel();

                await updater.TryUpdateModelAsync(model, Prefix);

                settings.Authority = model.Authority?.Trim();
                settings.Audience = model.Audience?.Trim();
                settings.Tenant = model.Tenant;

                foreach (var result in await _validationService.ValidateSettingsAsync(settings))
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

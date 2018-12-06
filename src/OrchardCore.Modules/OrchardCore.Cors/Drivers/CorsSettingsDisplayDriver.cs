using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Cors.Settings;
using OrchardCore.Cors.ViewModels;
using OrchardCore.Settings;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Cors.Drivers
{
    public class CorsSettingsDisplayDriver : SectionDisplayDriver<ISite, CorsSettings>
    {
        private const string RestartPendingCacheKey = "CorsSiteSettings_RestartPending";
        private const string SettingsGroupId = "Cors";

        private readonly INotifier _notifier;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorsSettingsDisplayDriver(INotifier notifier,
            IMemoryCache memoryCache,
            IHtmlLocalizer<CorsSettingsDisplayDriver> stringLocalizer,
            IHttpContextAccessor httpContextAccessor)
        {
            _notifier = notifier;
            _memoryCache = memoryCache;
            _httpContextAccessor = httpContextAccessor;
            T = stringLocalizer;
        }
        IHtmlLocalizer T { get; }

        public override IDisplayResult Edit(CorsSettings settings, BuildEditorContext context)
        {
            return Initialize<CorsSettingsViewModel>("CorsSettings_Edit", model =>
            {
                if (context.GroupId == SettingsGroupId && _memoryCache.Get(RestartPendingCacheKey) != null)
                    _notifier.Warning(T["The site needs to be restarted for the settings to take effect"]);

                var list = new List<CorsPolicyViewModel>();

                if (settings?.Policies != null)
                {
                    foreach (var policySetting in settings.Policies)
                    {
                        var policyViewModel = new CorsPolicyViewModel()
                        {
                            Name = policySetting.Name,
                            AllowAnyHeader = policySetting.AllowAnyHeader,
                            AllowedHeaders = policySetting.AllowedHeaders,
                            AllowAnyMethod = policySetting.AllowAnyMethod,
                            AllowedMethods = policySetting.AllowedMethods,
                            AllowAnyOrigin = policySetting.AllowAnyOrigin,
                            AllowedOrigins = policySetting.AllowedOrigins,
                            AllowCredentials = policySetting.AllowCredentials
                        };

                        list.Add(policyViewModel);
                    }
                }

                model.Policies = list.ToArray();
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(CorsSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == SettingsGroupId)
            {
                var model = new CorsSettingsViewModel();
                var request = _httpContextAccessor.HttpContext.Request;
                var configJson = request.Form["ISite.Policies"].First();
                model.Policies = JsonConvert.DeserializeObject<CorsPolicyViewModel[]>(configJson);

                settings.Policies = model.Policies.Select(p =>
                new CorsPolicySetting()
                {
                    Name = p.Name,
                    AllowAnyHeader = p.AllowAnyHeader,
                    AllowAnyMethod = p.AllowAnyMethod,
                    AllowAnyOrigin = p.AllowAnyOrigin,
                    AllowCredentials = p.AllowCredentials,
                    AllowedHeaders = p.AllowedHeaders,
                    AllowedMethods = p.AllowedMethods,
                    AllowedOrigins = p.AllowedOrigins
                }).ToArray();
            }

            if (_memoryCache.Get(RestartPendingCacheKey) == null)
            {
                var entry = _memoryCache.CreateEntry(RestartPendingCacheKey);
                _memoryCache.Set(entry.Key, entry, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
            }

            return await EditAsync(settings, context);
        }
    }
}

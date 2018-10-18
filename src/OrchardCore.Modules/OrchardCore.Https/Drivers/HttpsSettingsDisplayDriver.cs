using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Https.Settings;
using OrchardCore.Https.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Https.Drivers
{
    public class HttpsSettingsDisplayDriver : SectionDisplayDriver<ISite, HttpsSettings>
    {
        private const string RestartPendingCacheKey = "HttpsSiteSettings_RestartPending";
        private const string SettingsGroupId = "Https";
        private const int DefaultSslPort = 443;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IMemoryCache _memoryCache;

        public HttpsSettingsDisplayDriver(IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IMemoryCache memoryCache, 
            IHtmlLocalizer<HttpsSettingsDisplayDriver> stringLocalizer)
        {
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _memoryCache = memoryCache;
            T = stringLocalizer;
        }
        IHtmlLocalizer T { get; }

        public override IDisplayResult Edit(HttpsSettings settings, BuildEditorContext context)
        {

            if (context.GroupId == SettingsGroupId && _memoryCache.Get(RestartPendingCacheKey) != null)
                _notifier.Warning(T["The site needs to be restarted for the settings to take effect"]);

            var isHttpsRequest = _httpContextAccessor.HttpContext.Request.IsHttps;

            if (!isHttpsRequest)
                _notifier.Warning(T["For safety, Enabling require HTTPS over HTTP has been prevented."]);

            return Initialize<HttpsSettingsViewModel>("HttpsSettings_Edit", model =>
            {
                model.IsHttpsRequest = isHttpsRequest;
                model.RequireHttps = settings.RequireHttps;
                model.RequireHttpsPermanent = settings.RequireHttpsPermanent;
                model.SslPort = settings.SslPort ??
                                (_httpContextAccessor.HttpContext.Request.Host.Port == DefaultSslPort
                                    ? null
                                    : _httpContextAccessor.HttpContext.Request.Host.Port);
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(HttpsSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == SettingsGroupId)
            {
                var model = new HttpsSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.RequireHttps = model.RequireHttps;
                settings.RequireHttpsPermanent = model.RequireHttpsPermanent;
                settings.SslPort = model.SslPort;
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

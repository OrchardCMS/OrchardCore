using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.ReverseProxy.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy.Drivers
{
    public class ReverseProxySettingsDisplayDriver : SectionDisplayDriver<ISite, ReverseProxySettings>
    {
        private const string RestartPendingCacheKey = "ReverseProxySiteSettings_RestartPending";
        private const string SettingsGroupId = "ReverseProxy";

        private readonly INotifier _notifier;
        private readonly IMemoryCache _memoryCache;

        public ReverseProxySettingsDisplayDriver(INotifier notifier,
            IMemoryCache memoryCache,
            IHtmlLocalizer<ReverseProxySettingsDisplayDriver> stringLocalizer)
        {
            _notifier = notifier;
            _memoryCache = memoryCache;
            T = stringLocalizer;
        }
        IHtmlLocalizer T { get; }

        public override IDisplayResult Edit(ReverseProxySettings settings, BuildEditorContext context)
        {
            return Initialize<ReverseProxySettingsViewModel>("ReverseProxySettings_Edit", model =>
            {
                if (_memoryCache.Get(RestartPendingCacheKey) != null)
                    _notifier.Warning(T["The site needs to be restarted for the settings to take effect"]);

                model.EnableXForwardedFor = settings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor);
                model.EnableXForwardedHost = settings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost);
                model.EnableXForwardedProto = settings.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto);
            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ReverseProxySettings settings, BuildEditorContext context)
        {
            if (context.GroupId == SettingsGroupId)
            {
                var model = new ReverseProxySettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.ForwardedHeaders = ForwardedHeaders.None;

                if (model.EnableXForwardedFor)
                    settings.ForwardedHeaders |= ForwardedHeaders.XForwardedFor;

                if (model.EnableXForwardedHost)
                    settings.ForwardedHeaders |= ForwardedHeaders.XForwardedHost;

                if (model.EnableXForwardedProto)
                    settings.ForwardedHeaders |= ForwardedHeaders.XForwardedProto;

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

using Microsoft.Extensions.Caching.Memory;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Settings.Services
{
    public class SiteService : ISiteService
    {
        private readonly IContentManager _contentManager;
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;

        private const string SiteCacheKey = "Site";

        public SiteService(
            ISession session,
            IContentManager contentManager,
            IMemoryCache memoryCache)
        {
            _contentManager = contentManager;
            _session = session;
            _memoryCache = memoryCache;
        }

        public async Task<ISite> GetSiteSettingsAsync()
        {
            ContentItem site;

            if (!_memoryCache.TryGetValue(SiteCacheKey, out site))
            {
                site = await _session.QueryAsync<ContentItem, ContentItemIndex>(x => x.ContentType == "Site").FirstOrDefault();

                if (site == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(SiteCacheKey, out site))
                        {
                            site = _contentManager.New("Site");
                            site.Weld(new SiteSettingsPart()
                            {
                                SiteSalt = Guid.NewGuid().ToString("N"),
                                SiteName = "My Orchard Project Application",
                                PageTitleSeparator = " - ",
                                TimeZone = TimeZoneInfo.Local.Id,
                                PageSize = 10,
                                MaxPageSize = 100,
                                MaxPagedCount = 500
                            });

                            _contentManager.Create(site);
                        }
                    }
                }

                _memoryCache.Set(SiteCacheKey, site);
            }

            return site.As<SiteSettingsPart>();
        }

        public Task UpdateSiteSettingsAsync(ISite site)
        {
            _session.Save(site.ContentItem);
            _memoryCache.Set(SiteCacheKey, site.ContentItem);
            return Task.CompletedTask;
        }
    }
}

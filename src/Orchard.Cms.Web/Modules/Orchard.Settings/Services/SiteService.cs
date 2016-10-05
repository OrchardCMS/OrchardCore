using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using YesSql.Core.Services;

namespace Orchard.Settings.Services
{
    /// <summary>
    /// Implements <see cref="ISiteService"/> by storing the site as a Content Item.
    /// </summary>
    public class SiteService : ISiteService
    {
        private readonly IContentManager _contentManager;
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;

        private const string SiteCacheKey = "Site";
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public SiteService(
            ISession session,
            IContentManager contentManager,
            IMemoryCache memoryCache,
            IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _session = session;
            _memoryCache = memoryCache;
        }

        /// <inheritdoc/>
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
                            // Ensure the content type exists
                            _contentDefinitionManager.AlterTypeDefinition("Site", builder => { });

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
                            _memoryCache.Set(SiteCacheKey, site);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(SiteCacheKey, site);
                }
            }

            return site.As<SiteSettingsPart>();
        }

        /// <inheritdoc/>
        public Task UpdateSiteSettingsAsync(ISite site)
        {
            var siteSettingsPart = site as SiteSettingsPart;
            var contentItem = siteSettingsPart.ContentItem;
            contentItem.Update(siteSettingsPart);

            _session.Save(contentItem);
            _memoryCache.Set(SiteCacheKey, contentItem);
            return Task.CompletedTask;
        }
    }
}

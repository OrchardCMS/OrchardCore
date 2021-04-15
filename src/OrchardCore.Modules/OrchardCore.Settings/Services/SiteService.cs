using System;
using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Modules;

namespace OrchardCore.Settings.Services
{
    /// <summary>
    /// Implements <see cref="ISiteService"/> by storing the site settings as a document.
    /// </summary>
    public class SiteService : ISiteService
    {
        private readonly IDocumentManager<SiteSettings> _documentManager;
        private readonly IClock _clock;

        public SiteService(IDocumentManager<SiteSettings> documentManager, IClock clock)
        {
            _documentManager = documentManager;
            _clock = clock;
        }

        /// <summary>
        /// Loads the site settings from the store for updating and that should not be cached.
        /// </summary>
        // Await as we can't cast 'Task<SiteSettings>' to 'Task<ISite>'.
        public async Task<ISite> LoadSiteSettingsAsync() => await _documentManager.GetOrCreateMutableAsync(GetDefaultSettingsAsync);

        /// <summary>
        /// Gets the site settings from the cache for sharing and that should not be updated.
        /// </summary>
        // Await as we can't cast 'Task<SiteSettings>' to 'Task<ISite>'.
        public async Task<ISite> GetSiteSettingsAsync() => await _documentManager.GetOrCreateImmutableAsync(GetDefaultSettingsAsync);

        /// <summary>
        /// Updates the store with the provided site settings and then updates the cache.
        /// </summary>
        public Task UpdateSiteSettingsAsync(ISite site) => _documentManager.UpdateAsync(site as SiteSettings);

        private Task<SiteSettings> GetDefaultSettingsAsync()
        {
            return Task.FromResult(new SiteSettings
            {
                SiteSalt = Guid.NewGuid().ToString("N"),
                SiteName = "My Orchard Project Application",
                PageTitleFormat = "{% page_title Site.SiteName, position: \"after\", separator: \" - \" %}",
                TimeZoneId = _clock.GetSystemTimeZone().TimeZoneId,
                PageSize = 10,
                MaxPageSize = 100,
                MaxPagedCount = 0
            });
        }
    }
}

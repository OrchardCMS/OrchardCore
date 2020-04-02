using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Settings.Services
{
    /// <summary>
    /// Implements <see cref="ISiteService"/> by storing the site as a Content Item.
    /// </summary>
    public class SiteService : ISiteService
    {
        private readonly IClock _clock;

        public SiteService(IClock clock) => _clock = clock;

        /// <summary>
        /// Loads the document from the store (or create a new one) for updating and that should not be cached.
        /// </summary>
        public async Task<ISite> LoadSiteSettingsAsync()
        {
            var site = await DocumentManager.GetMutableAsync(GetDefaultSettings);
            return site;
        }

        /// <summary>
        /// Gets the document from the cache (or create a new one) for sharing and that should not be updated.
        /// </summary>
        public async Task<ISite> GetSiteSettingsAsync()
        {
            var site = await DocumentManager.GetImmutableAsync(GetDefaultSettings);
            return site;
        }

        /// <summary>
        /// Updates the database with the provided document and then keeps in sync the cache.
        /// </summary>
        public Task UpdateSiteSettingsAsync(ISite site) => DocumentManager.UpdateAsync(site as SiteSettings);

        private SiteSettings GetDefaultSettings()
        {
            return new SiteSettings
            {
                SiteSalt = Guid.NewGuid().ToString("N"),
                SiteName = "My Orchard Project Application",
                PageTitleFormat = "{% page_title Site.SiteName, position: \"after\", separator: \" - \" %}",
                TimeZoneId = _clock.GetSystemTimeZone().TimeZoneId,
                PageSize = 10,
                MaxPageSize = 100,
                MaxPagedCount = 0
            };
        }

        private IDocumentManager<SiteSettings> DocumentManager => ShellScope.Services.GetRequiredService<IDocumentManager<SiteSettings>>();
    }
}

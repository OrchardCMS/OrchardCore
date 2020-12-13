using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Settings.Services
{
    /// <summary>
    /// Implements <see cref="ISiteService"/> by storing the site settings as a document.
    /// </summary>
    public class SiteService : ISiteService
    {
        private readonly IClock _clock;

        public SiteService(IClock clock) => _clock = clock;

        /// <summary>
        /// Loads the site settings from the store for updating and that should not be cached.
        /// </summary>
        // Await as we can't cast 'Task<SiteSettings>' to 'Task<ISite>'.
        public async Task<ISite> LoadSiteSettingsAsync() => await DocumentManager.GetOrCreateMutableAsync(GetDefaultSettingsAsync);

        /// <summary>
        /// Gets the site settings from the cache for sharing and that should not be updated.
        /// </summary>
        // Await as we can't cast 'Task<SiteSettings>' to 'Task<ISite>'.
        public async Task<ISite> GetSiteSettingsAsync() => await DocumentManager.GetOrCreateImmutableAsync(GetDefaultSettingsAsync);

        /// <summary>
        /// Updates the store with the provided site settings and then updates the cache.
        /// </summary>
        public Task UpdateSiteSettingsAsync(ISite site) => DocumentManager.UpdateAsync(site as SiteSettings);

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

        private static IDocumentManager<SiteSettings> DocumentManager => ShellScope.Services.GetRequiredService<IDocumentManager<SiteSettings>>();
    }
}

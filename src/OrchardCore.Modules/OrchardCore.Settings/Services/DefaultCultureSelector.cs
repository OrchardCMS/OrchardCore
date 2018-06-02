using System.Threading.Tasks;
using OrchardCore.Localization.Services;

namespace OrchardCore.Settings.Services
{
    /// <summary>
    /// Provides the timezone defined in the site configuration for the current scope (request).
    /// The same <see cref="TimeZoneSelectorResult"/> is returned if called multiple times 
    /// during the same scope.
    /// </summary>
    public class DefaultCultureSelector : ICultureSelector
    {
        private readonly ISiteService _siteService;

        public DefaultCultureSelector(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public Task<CultureSelectorResult> GetCultureAsync()
        {
            return Task.FromResult(new CultureSelectorResult
            {
                Priority = 0,
                Name = () => _siteService.GetSiteSettingsAsync().ContinueWith(x => x.Result?.Culture)
            });
        }
    }
}

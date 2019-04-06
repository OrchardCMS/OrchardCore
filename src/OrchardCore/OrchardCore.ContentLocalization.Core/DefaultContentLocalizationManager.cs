using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentLocalization.Handlers;
using OrchardCore.ContentManagement;
using YesSql;
using OrchardCore.Settings;
using OrchardCore.Modules;
using Microsoft.Extensions.Logging;

namespace OrchardCore.ContentLocalization
{
    public class DefaultContentLocalizationManager : IContentLocalizationManager
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly ISiteService _siteService;
        private readonly ILogger<DefaultContentLocalizationManager> _logger;

        public IEnumerable<IContentLocalizationHandler> Handlers { get; private set; }
        public IEnumerable<IContentLocalizationHandler> ReversedHandlers { get; private set; }

        public DefaultContentLocalizationManager(IContentManager contentManager, ISession session, ISiteService siteService, ILogger<DefaultContentLocalizationManager> logger, IEnumerable<IContentLocalizationHandler> handlers)
        {
            _contentManager = contentManager;
            _session = session;
            _siteService = siteService;
            Handlers = handlers;
            ReversedHandlers = handlers.Reverse().ToArray();
            _logger = logger;
        }
        
        public async Task<ContentItem> GetContentItem(string localizationSet, string culture)
        {
            var invariantCulture = culture.ToLowerInvariant();
            var indexValue = await _session.Query<ContentItem, LocalizedContentItemIndex>(o =>
                o.LocalizationSet == localizationSet
                && o.Culture == invariantCulture
            ).FirstOrDefaultAsync();

            return indexValue;
        }

        public Task<IEnumerable<ContentItem>> GetItemsForSet(string localizationSet)
        {
            return _session.Query<ContentItem, LocalizedContentItemIndex>(o => o.LocalizationSet == localizationSet).ListAsync();

        }

        public async Task<ContentItem> LocalizeAsync(ContentItem content, string targetCulture)
        {
           var localizationPart = content.As<LocalizationPart>();
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            // not sure if this is redundant or not. The check is also done in the Admin controller
            if(!siteSettings.SupportedCultures.Any(c=>String.Equals(c,targetCulture, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new NotSupportedException("Cannot localize an unsupported culture");
            }
            // not sure if this is redundant or not. The check is also done in the Admin controller
            var existingContent = await GetContentItem(localizationPart.LocalizationSet, targetCulture);
            if (existingContent != null)
            {
                // already localized
                return existingContent;
            }

            var cloned = await _contentManager.CloneAsync(content);
            var clonedPart = cloned.As<LocalizationPart>();
            clonedPart.Culture = targetCulture;
            clonedPart.Apply();

            var context = new LocalizationContentContext(content, localizationPart.LocalizationSet, targetCulture);

            await Handlers.InvokeAsync(async handler => await handler.LocalizingAsync(context), _logger);
            await ReversedHandlers.InvokeAsync(async handler => await handler.LocalizedAsync(context), _logger);

            _session.Save(cloned);
            return cloned;

        }
    }
}

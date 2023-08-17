using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentLocalization.Handlers;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Localization;
using OrchardCore.Modules;
using YesSql;
using YesSql.Services;

namespace OrchardCore.ContentLocalization
{
    public class DefaultContentLocalizationManager : IContentLocalizationManager
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly Entities.IIdGenerator _iidGenerator;

        public IEnumerable<IContentLocalizationHandler> Handlers { get; private set; }
        public IEnumerable<IContentLocalizationHandler> ReversedHandlers { get; private set; }

        public DefaultContentLocalizationManager(
            IContentManager contentManager,
            ISession session,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContentAccessor,
            ILocalizationService localizationService,
            ILogger<DefaultContentLocalizationManager> logger,
            IEnumerable<IContentLocalizationHandler> handlers,
            Entities.IIdGenerator iidGenerator)
        {
            _contentManager = contentManager;
            _session = session;
            _httpContextAccessor = httpContentAccessor;
            _localizationService = localizationService;
            Handlers = handlers;
            _iidGenerator = iidGenerator;
            ReversedHandlers = handlers.Reverse().ToArray();
            _logger = logger;
        }

        public Task<ContentItem> GetContentItemAsync(string localizationSet, string culture)
        {
            var invariantCulture = culture.ToLowerInvariant();
            return _session.Query<ContentItem, LocalizedContentItemIndex>(i =>
                        (i.Published || i.Latest) &&
                        i.LocalizationSet == localizationSet &&
                        i.Culture == invariantCulture)
                    .FirstOrDefaultAsync();
        }

        public Task<IEnumerable<ContentItem>> GetItemsForSetAsync(string localizationSet)
        {
            return _session.Query<ContentItem, LocalizedContentItemIndex>(i => (i.Published || i.Latest) && i.LocalizationSet == localizationSet).ListAsync();
        }

        public Task<IEnumerable<ContentItem>> GetItemsForSetsAsync(IEnumerable<string> localizationSets, string culture)
        {
            var invariantCulture = culture.ToLowerInvariant();
            return _session.Query<ContentItem, LocalizedContentItemIndex>(i => (i.Published || i.Latest) && i.LocalizationSet.IsIn(localizationSets) && i.Culture == invariantCulture).ListAsync();
        }

        public async Task<ContentItem> LocalizeAsync(ContentItem content, string targetCulture)
        {
            var supportedCultures = await _localizationService.GetSupportedCulturesAsync();
            if (!supportedCultures.Any(c => String.Equals(c, targetCulture, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Cannot localize an unsupported culture");
            }

            var localizationPart = content.As<LocalizationPart>();
            if (String.IsNullOrEmpty(localizationPart.LocalizationSet))
            {
                // If the source content item is not yet localized, define its defaults.
                localizationPart.LocalizationSet = _iidGenerator.GenerateUniqueId();
                localizationPart.Culture = await _localizationService.GetDefaultCultureAsync();
                _session.Save(content);
            }
            else
            {
                var existingContent = await GetContentItemAsync(localizationPart.LocalizationSet, targetCulture);

                if (existingContent != null)
                {
                    // Already localized.
                    return existingContent;
                }
            }

            // Cloning the content item.
            var cloned = await _contentManager.CloneAsync(content);
            var clonedPart = cloned.As<LocalizationPart>();
            clonedPart.Culture = targetCulture;
            clonedPart.LocalizationSet = localizationPart.LocalizationSet;
            clonedPart.Apply();

            var context = new LocalizationContentContext(cloned, content, localizationPart.LocalizationSet, targetCulture);

            await Handlers.InvokeAsync((handler, context) => handler.LocalizingAsync(context), context, _logger);
            await ReversedHandlers.InvokeAsync((handler, context) => handler.LocalizedAsync(context), context, _logger);

            return cloned;
        }

        public async Task<IDictionary<string, ContentItem>> DeduplicateContentItemsAsync(IEnumerable<ContentItem> contentItems)
        {
            var contentItemIds = contentItems.Select(c => c.ContentItemId);
            var indexValues = await _session.QueryIndex<LocalizedContentItemIndex>(i => (i.Published || i.Latest) && i.ContentItemId.IsIn(contentItemIds)).ListAsync();

            var currentCulture = _httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name.ToLowerInvariant();
            var defaultCulture = (await _localizationService.GetDefaultCultureAsync()).ToLowerInvariant();
            var cleanedIndexValues = GetSingleContentItemIdPerSet(indexValues, currentCulture, defaultCulture);

            var dictionary = new Dictionary<string, ContentItem>();
            foreach (var val in cleanedIndexValues)
            {
                dictionary.Add(val.LocalizationSet, contentItems.SingleOrDefault(ci => ci.ContentItemId == val.ContentItemId));
            }

            return dictionary;
        }

        public async Task<IDictionary<string, string>> GetFirstItemIdForSetsAsync(IEnumerable<string> localizationSets)
        {
            var indexValues = await _session.QueryIndex<LocalizedContentItemIndex>(i => (i.Published || i.Latest) && i.LocalizationSet.IsIn(localizationSets)).ListAsync();

            var currentCulture = _httpContextAccessor.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name.ToLowerInvariant();
            var defaultCulture = (await _localizationService.GetDefaultCultureAsync()).ToLowerInvariant();
            var dictionary = new Dictionary<string, string>();
            var cleanedIndexValues = GetSingleContentItemIdPerSet(indexValues, currentCulture, defaultCulture);

            // This loop keeps the original ordering of localizationSets for the LocalizationSetContentPicker.
            foreach (var set in localizationSets)
            {
                var idxValue = cleanedIndexValues.FirstOrDefault(x => x.LocalizationSet == set);
                if (idxValue == null)
                {
                    continue;
                }

                dictionary.Add(idxValue.LocalizationSet, idxValue.ContentItemId);
            }

            return dictionary;
        }

        /// <summary>
        /// ContentItemId chosen with the following rules:
        /// ContentItemId of the current culture for the set
        /// OR ContentItemId of the default culture for the set
        /// OR First ContentItemId found in the set
        /// OR null if nothing found.
        /// </summary>
        /// <returns>List of ContentItemId.</returns>
        private static IEnumerable<LocalizedContentItemIndex> GetSingleContentItemIdPerSet(IEnumerable<LocalizedContentItemIndex> indexValues, string currentCulture, string defaultCulture)
        {
            return indexValues.GroupBy(l => l.LocalizationSet).Select(set =>
            {
                var currentcultureContentItem = set.FirstOrDefault(f => f.Culture == currentCulture);
                if (currentcultureContentItem is not null)
                {
                    return currentcultureContentItem;
                }

                var defaultCultureContentItem = set.FirstOrDefault(f => f.Culture == defaultCulture);
                if (defaultCultureContentItem is not null)
                {
                    return defaultCultureContentItem;
                }

                if (set.Any())
                {
                    return set.FirstOrDefault();
                }

                return null;
            }).OfType<LocalizedContentItemIndex>().ToList();
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    public class ContentTypeDefinitionDataLocalizerFactory : IDataLocalizerFactory
    {
        private readonly ConcurrentDictionary<string, ContentTypeDefinitionDataLocalizer> _localizerCache = new ConcurrentDictionary<string, ContentTypeDefinitionDataLocalizer>();
        private readonly IContentDefinitionStore _contentDefinitionStore;
        private readonly bool _fallBackToParentUICultures;
        private readonly ILoggerFactory _loggerFactory;

        public ContentTypeDefinitionDataLocalizerFactory(
            IHttpContextAccessor httpContextAccessor,
            IOptions<RequestLocalizationOptions> requestLocalizationOptions,
            ILoggerFactory loggerFactory)
        {
            _contentDefinitionStore = httpContextAccessor?.HttpContext.RequestServices.GetService<IContentDefinitionStore>() ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _fallBackToParentUICultures = requestLocalizationOptions?.Value.FallBackToParentUICultures ?? throw new ArgumentNullException(nameof(requestLocalizationOptions));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IDataLocalizer Create()
        {
            var culture = CultureInfo.CurrentUICulture;

            return _localizerCache.GetOrAdd($"C={culture.Name}", _ =>
            {
                var resourceDictionary = GetResourceDictionary(culture).GetAwaiter().GetResult();
                var resourceManager = new DataResourceManager(resourceDictionary, _fallBackToParentUICultures);

                return new ContentTypeDefinitionDataLocalizer(resourceManager, _loggerFactory.CreateLogger<DataLocalizer>());
            });
        }

        private async Task<IEnumerable<CultureDictionary>> GetResourceDictionary(CultureInfo culture)
        {
            var currentCulture = culture;
            var dictionaries = new List<CultureDictionary>();
            var contentDefinition = await _contentDefinitionStore.LoadContentDefinitionAsync();

            do
            {
                var records = contentDefinition.ContentTypeDefinitionRecords
                    .Where(r => r.DisplayName.GetValueOrDefault(currentCulture.Name) != null)
                    .Select(r => new CultureDictionaryRecord(
                        r.DisplayName.Default,
                        new string[] { r.DisplayName.GetValueOrDefault(currentCulture.Name) }));

                if (records != null)
                {
                    var dictionary = new CultureDictionary(currentCulture.Name, null);
                    dictionary.MergeTranslations(records);
                    dictionaries.Add(dictionary);
                }

                currentCulture = currentCulture.Parent;
            }
            while (currentCulture != currentCulture.Parent);

            return dictionaries;
        }
    }
}
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fluid;

using OrchardCore.ContentLocalization.Records;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentLocalization.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Liquid;
using YesSql;

namespace OrchardCore.ContentLocalization.Handlers
{
    public class LocalizationPartHandler : ContentPartHandler<LocalizationPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILocalizationEntries _entries;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly ISession _session;

        public LocalizationPartHandler(ILocalizationEntries entries,
            IContentDefinitionManager contentDefinitionManager,
            ILiquidTemplateManager liquidTemplateManager, ISession session)
        {
            _entries = entries;
            _contentDefinitionManager = contentDefinitionManager;
            _liquidTemplateManager = liquidTemplateManager;
            _session = session;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, LocalizationPart part)
        {
            return context.ForAsync<CultureAspect>(cultureAspect =>
            {
                cultureAspect.Culture = CultureInfo.GetCultureInfo(part.Culture);
                return Task.CompletedTask;
            });
        }

        public override Task PublishedAsync(PublishContentContext context, LocalizationPart part)
        {
            if (!string.IsNullOrWhiteSpace(part.LocalizationSet))
            {
                _entries.AddEntry(new LocalizationEntry()
                {
                    ContentItemId = part.ContentItem.ContentItemId,
                    LocalizationSet = part.LocalizationSet,
                    Culture = part.Culture.ToLowerInvariant()
                });
            }

            return Task.CompletedTask;
        }

        public override Task UnpublishedAsync(PublishContentContext context, LocalizationPart part)
        {
            _entries.RemoveEntry(new LocalizationEntry()
            {
                ContentItemId = part.ContentItem.ContentItemId,
                LocalizationSet = part.LocalizationSet,
                Culture = part.Culture.ToLowerInvariant()
            });

            return Task.CompletedTask;
        }

        public override Task RemovedAsync(RemoveContentContext context, LocalizationPart part)
        {
            _entries.RemoveEntry(new LocalizationEntry()
            {
                ContentItemId = part.ContentItem.ContentItemId,
                LocalizationSet = part.LocalizationSet,
                Culture = part.Culture.ToLowerInvariant()
            });

            return Task.CompletedTask;
        }

        public override async Task UpdatedAsync(UpdateContentContext context, LocalizationPart part)
        {
            var pattern = GetPattern(part);

            if (!string.IsNullOrEmpty(pattern))
            {
                var templateContext = new TemplateContext();
                templateContext.SetValue("ContentItem", part.ContentItem);

                var json = await _liquidTemplateManager.RenderAsync(pattern, NullEncoder.Default, templateContext);

                var jObject = JObject.Parse(json);
                // this does not call UpdatedAsync on all Related LocalizationPart handlers.
                // If it did, and infinite loop would occur.
                var items = await _session.Query<ContentItem, LocalizedContentItemIndex>(o => o.LocalizationSet == part.LocalizationSet).ListAsync();

                foreach (var item in items)
                {
                    var content = (JObject)item.ContentItem.Content;
                    content.Merge(jObject, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
                    _session.Save(item);
                }
            }
        }

        /// <summary>
        /// Get the pattern from the LocalizationPartSettings property for its type
        /// </summary>
        private string GetPattern(LocalizationPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => string.Equals(x.PartDefinition.Name, nameof(LocalizationPart), StringComparison.Ordinal));
            return contentTypePartDefinition.GetSettings<LocalizationPartSettings>().Pattern;
        }
    }
}

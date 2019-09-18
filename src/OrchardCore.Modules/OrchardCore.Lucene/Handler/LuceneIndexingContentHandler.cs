using System;
using System.Text;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Modules;

namespace OrchardCore.Lucene.Handlers
{
    public class LuceneIndexingContentHandler : ContentHandlerBase
    {
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LuceneIndexingContentHandler> _logger;

        public LuceneIndexingContentHandler(
            LuceneIndexManager luceneIndexManager,
            IContentDefinitionManager contentDefinitionManager,
            ILiquidTemplateManager liquidTemplateManager,
            IServiceProvider serviceProvider,
            ILogger<LuceneIndexingContentHandler> logger)
        {
            _luceneIndexManager = luceneIndexManager;
            _contentDefinitionManager = contentDefinitionManager;
            _liquidTemplateManager = liquidTemplateManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            var buildIndexContext = new BuildIndexContext(new DocumentIndex(context.ContentItem.ContentItemId), context.ContentItem, new string[] { context.ContentItem.ContentType });
            // Lazy resolution to prevent cyclic dependency 
            var contentItemIndexHandlers = _serviceProvider.GetServices<IContentItemIndexHandler>();
            await contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
                _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
            }
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
            }

            return Task.CompletedTask;
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            // TODO: ignore if this index is not configured for the content type

            foreach (var index in _luceneIndexManager.List())
            {
                _luceneIndexManager.DeleteDocuments(index, new string[] { context.ContentItem.ContentItemId });
            }

            return Task.CompletedTask;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            return context.ForAsync<FullTextAspect>(async fullTextAspect =>
            {
                var sb = new StringBuilder();
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);

                if (contentTypeDefinition == null)
                {
                    return;
                }

                //We always index DisplayText in FullText
                fullTextAspect.FullText = sb.AppendLine(context.ContentItem.DisplayText);

                //We always index BodyPart in FullText
                var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
                var body = await contentManager.PopulateAspectAsync(context.ContentItem, new BodyAspect());

                if (body != null)
                {
                    fullTextAspect.FullText.AppendLine(body.Body.ToString());
                }

                //We index values from custom FullText settings
                var settings = contentTypeDefinition.GetSettings<ContentTypeIndexingSettings>();

                if (settings.IsFullText && !String.IsNullOrEmpty(settings.FullText))
                {
                    var templateContext = new TemplateContext();
                    templateContext.SetValue("Model", context.ContentItem);

                    var result = await _liquidTemplateManager.RenderAsync(settings.FullText, NullEncoder.Default, templateContext);
                    fullTextAspect.FullText.AppendLine(result);
                }
            });
        }
    }
}
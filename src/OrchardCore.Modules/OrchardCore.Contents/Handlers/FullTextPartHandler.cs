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
using OrchardCore.Contents.Models;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Handlers
{
    public class FullTextPartHandler : ContentPartHandler<FullTextPart>
    {

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FullTextPartHandler> _logger;

        public FullTextPartHandler(
            IContentDefinitionManager contentDefinitionManager,
            ILiquidTemplateManager liquidTemplateManager,
            IServiceProvider serviceProvider,
            ILogger<FullTextPartHandler> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _liquidTemplateManager = liquidTemplateManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, FullTextPart part)
        {
            return context.ForAsync<FullTextAspect>(async fullTextAspect =>
            {
                var sb = new StringBuilder();
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);

                if (contentTypeDefinition == null)
                {
                    return;
                }

                if (part.IsIndexed)
                {
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
                }
            });
        }
    }
}

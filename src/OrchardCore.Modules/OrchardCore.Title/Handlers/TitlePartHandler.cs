using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Liquid;
using OrchardCore.Title.Models;
using OrchardCore.Title.ViewModels;

namespace OrchardCore.Title.Handlers
{
    public class TitlePartHandler : ContentPartHandler<TitlePart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public TitlePartHandler(
            ILiquidTemplateManager liquidTemplateManager,
            IContentDefinitionManager contentDefinitionManager)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override async Task UpdatedAsync(UpdateContentContext context, TitlePart part)
        {
            var settings = GetSettings(part);
            // Do not compute the title if the user can modify it and the text is already set.
            if (settings.Options == TitlePartOptions.Editable && !String.IsNullOrWhiteSpace(part.ContentItem.DisplayText))
            {
                return;
            }

            if (!String.IsNullOrEmpty(settings.Pattern))
            {
                var model = new TitlePartViewModel()
                {
                    Title = part.Title,
                    TitlePart = part,
                    ContentItem = part.ContentItem
                };

                var title = await _liquidTemplateManager.RenderAsync(settings.Pattern, NullEncoder.Default, model,
                    scope => scope.SetValue("ContentItem", model.ContentItem));

                title = title.Replace("\r", String.Empty).Replace("\n", String.Empty);

                part.Title = title;
                part.ContentItem.DisplayText = title;
                part.Apply();
            }
        }

        private TitlePartSettings GetSettings(TitlePart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, nameof(TitlePart)));
            return contentTypePartDefinition.GetSettings<TitlePartSettings>();
        }
    }
}

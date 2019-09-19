using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.ContentTypes.Editors
{
    public class ContentTypeIndexingSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
    {

        private readonly ILiquidTemplateManager _templateManager;

        public ContentTypeIndexingSettingsDisplayDriver(
            ILiquidTemplateManager templateManager,
            IStringLocalizer<ContentTypeIndexingSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            T = localizer;
        }

        public IStringLocalizer T { get; private set; }

        public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
        {
            return Initialize<ContentTypeIndexingSettingsViewModel>("ContentTypeIndexingSettings_Edit", model =>
            {
                var settings = contentTypeDefinition.GetSettings<ContentTypeIndexingSettings>();

                model.IsFullTextLiquid = settings.IsFullText;
                model.FullTextLiquid = settings.FullText;
                model.IndexDisplayText = settings.IndexDisplayText;
                model.IndexBodyAspect = settings.IndexBodyAspect;
            }).Location("Content:6");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
        {
            var model = new ContentTypeIndexingSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix, 
                m => m.IsFullTextLiquid,
                m => m.FullTextLiquid);

            if (!string.IsNullOrEmpty(model.FullTextLiquid) && !_templateManager.Validate(model.FullTextLiquid, out var errors))
            {
                context.Updater.ModelState.AddModelError(nameof(model.FullTextLiquid), T["Full-text doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            } 
            else 
            {
                context.Builder.WithSettings(new ContentTypeIndexingSettings
                {
                    IsFullText = model.IsFullTextLiquid,
                    FullText = model.FullTextLiquid,
                    IndexDisplayText = model.IndexDisplayText,
                    IndexBodyAspect = model.IndexBodyAspect
                });
            }

            return Edit(contentTypeDefinition, context.Updater);
        }
    }
}

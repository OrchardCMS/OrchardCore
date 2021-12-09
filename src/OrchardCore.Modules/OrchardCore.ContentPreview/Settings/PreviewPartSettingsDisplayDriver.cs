using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentPreview.Models;
using OrchardCore.ContentPreview.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;

namespace OrchardCore.ContentPreview.Settings
{
    public class PreviewPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<PreviewPart>
    {
        private readonly ILiquidTemplateManager _templateManager;
        private readonly IStringLocalizer S;

        public PreviewPartSettingsDisplayDriver(ILiquidTemplateManager templateManager, IStringLocalizer<PreviewPartSettingsDisplayDriver> localizer)
        {
            _templateManager = templateManager;
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<PreviewPartSettingsViewModel>("PreviewPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<PreviewPartSettings>();

                model.Pattern = settings.Pattern;
                model.PreviewPartSettings = settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            var model = new PreviewPartSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix,
                m => m.Pattern
                );

            if (!string.IsNullOrEmpty(model.Pattern) && !_templateManager.Validate(model.Pattern, out var errors))
            {
                context.Updater.ModelState.AddModelError(nameof(model.Pattern), S["Pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            }
            else
            {
                context.Builder.WithSettings(new PreviewPartSettings
                {
                    Pattern = model.Pattern
                });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}

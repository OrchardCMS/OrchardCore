using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.ViewModels;

namespace OrchardCore.Menu.Settings
{
    public class HtmlMenuItemPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<HtmlMenuItemPart>
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<HtmlMenuItemPartSettingsViewModel>("HtmlMenuItemPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<HtmlMenuItemPartSettings>();

                model.SanitizeHtml = settings.SanitizeHtml;
            })
            .Location("Content:20");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            var model = new HtmlMenuItemPartSettingsViewModel();
            var settings = new HtmlMenuItemPartSettings();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                settings.SanitizeHtml = model.SanitizeHtml;

                context.Builder.WithSettings(settings);
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}

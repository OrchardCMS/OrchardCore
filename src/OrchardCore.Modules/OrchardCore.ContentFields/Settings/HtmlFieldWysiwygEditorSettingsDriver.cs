using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class HtmlFieldWysiwygEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<HtmlField>
    {
        public HtmlFieldWysiwygEditorSettingsDriver(IStringLocalizer<HtmlFieldWysiwygEditorSettingsDriver> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<WysiwygSettingsViewModel>("HtmlFieldWysiwygEditorSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<HtmlFieldWysiwygEditorSettings>();
                
                model.Options = settings.Options;
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Wysiwyg")
            {
                var model = new WysiwygSettingsViewModel();
                var settings = new HtmlFieldWysiwygEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.Options = model.Options;

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition);
        }
    }
}

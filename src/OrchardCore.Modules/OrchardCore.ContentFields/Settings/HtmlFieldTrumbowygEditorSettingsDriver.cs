using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.ContentFields.Settings
{
    public class HtmlFieldTrumbowygEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<HtmlField>
    {
        protected readonly IStringLocalizer S;

        public HtmlFieldTrumbowygEditorSettingsDriver(IStringLocalizer<HtmlFieldTrumbowygEditorSettingsDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<TrumbowygSettingsViewModel>("HtmlFieldTrumbowygEditorSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<HtmlFieldTrumbowygEditorSettings>();

                model.Options = settings.Options;
                model.InsertMediaWithUrl = settings.InsertMediaWithUrl;
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Trumbowyg")
            {
                var model = new TrumbowygSettingsViewModel();
                var settings = new HtmlFieldTrumbowygEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (!model.Options.IsJson())
                {
                    context.Updater.ModelState.AddModelError(Prefix + '.' + nameof(TrumbowygSettingsViewModel.Options), S["The options are written in an incorrect format."]);
                }
                else
                {
                    settings.InsertMediaWithUrl = model.InsertMediaWithUrl;
                    settings.Options = model.Options;

                    context.Builder.WithSettings(settings);
                }
            }

            return Edit(partFieldDefinition);
        }
    }
}

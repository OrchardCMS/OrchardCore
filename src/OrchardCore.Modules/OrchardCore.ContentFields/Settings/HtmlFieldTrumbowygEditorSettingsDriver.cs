using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class HtmlFieldTrumbowygEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<HtmlField>
    {
        private readonly IStringLocalizer S;

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

                settings.InsertMediaWithUrl = model.InsertMediaWithUrl;

                try
                {
                    settings.Options = model.Options;
                    JObject.Parse(settings.Options);
                }
                catch
                {
                    context.Updater.ModelState.AddModelError(Prefix, S["The options are written in an incorrect format."]);
                    return Edit(partFieldDefinition);
                }

                context.Builder.WithSettings(settings);
            }

            return Edit(partFieldDefinition);
        }
    }
}

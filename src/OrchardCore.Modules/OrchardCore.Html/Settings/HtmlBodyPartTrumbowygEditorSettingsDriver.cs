using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.ViewModels;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Html.Settings
{
    public class HtmlBodyPartTrumbowygEditorSettingsDriver : ContentTypePartDefinitionDisplayDriver<HtmlBodyPart>
    {
        protected readonly IStringLocalizer S;

        public HtmlBodyPartTrumbowygEditorSettingsDriver(IStringLocalizer<HtmlBodyPartTrumbowygEditorSettingsDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            return Initialize<TrumbowygSettingsViewModel>("HtmlBodyPartTrumbowygSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartTrumbowygEditorSettings>();

                model.Options = settings.Options;
                model.InsertMediaWithUrl = settings.InsertMediaWithUrl;
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (contentTypePartDefinition.Editor() == "Trumbowyg")
            {
                var model = new TrumbowygSettingsViewModel();
                var settings = new HtmlBodyPartTrumbowygEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (!model.Options.IsJson())
                {
                    context.Updater.ModelState.AddModelError(Prefix + "." + nameof(TrumbowygSettingsViewModel.Options), S["The options are written in an incorrect format."]);
                }
                else
                {
                    settings.InsertMediaWithUrl = model.InsertMediaWithUrl;
                    settings.Options = model.Options;
                    context.Builder.WithSettings(settings);
                }
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}

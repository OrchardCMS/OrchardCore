using System;
using System.Threading.Tasks;
using OrchardCore.Html.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.ViewModels;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Html.Settings
{
    public class HtmlBodyPartTrumbowygEditorSettingsDriver : ContentTypePartDefinitionDisplayDriver
    {
        public HtmlBodyPartTrumbowygEditorSettingsDriver(IStringLocalizer<HtmlBodyPartTrumbowygEditorSettingsDriver> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, IUpdateModel updater)
        {
            if (!String.Equals(nameof(HtmlBodyPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<TrumbowygSettingsViewModel>("HtmlBodyPartTrumbowygSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartTrumbowygEditorSettings>();

                model.Options = settings.Options;
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(HtmlBodyPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new TrumbowygSettingsViewModel();
            var settings = new HtmlBodyPartTrumbowygEditorSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            try
            {
                settings.Options = model.Options;
                JObject.Parse(settings.Options);
            }
            catch
            {
                context.Updater.ModelState.AddModelError(Prefix, T["The options are written in an incorrect format."]);
                return Edit(contentTypePartDefinition, context.Updater);
            }

            context.Builder.WithSettings(settings);

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}
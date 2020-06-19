using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.ViewModels;

namespace OrchardCore.Html.Settings
{
    public class HtmlBodyPartTrumbowygEditorSettingsDriver : ContentTypePartDefinitionDisplayDriver
    {
        private readonly IStringLocalizer S;

        public HtmlBodyPartTrumbowygEditorSettingsDriver(IStringLocalizer<HtmlBodyPartTrumbowygEditorSettingsDriver> localizer)
        {
            S = localizer;
        }

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
                model.InsertMediaWithUrl = settings.InsertMediaWithUrl;
            })
            .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(HtmlBodyPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            if (contentTypePartDefinition.Editor() == "Trumbowyg")
            {
                var model = new TrumbowygSettingsViewModel();
                var settings = new HtmlBodyPartTrumbowygEditorSettings();

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
                    return Edit(contentTypePartDefinition, context.Updater);
                }

                context.Builder.WithSettings(settings);
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}

using Acornima;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Html.Settings;

public sealed class HtmlBodyPartTrumbowygEditorSettingsDriver : ContentTypePartDefinitionDisplayDriver<HtmlBodyPart>
{
    internal readonly IStringLocalizer S;

    public HtmlBodyPartTrumbowygEditorSettingsDriver(IStringLocalizer<HtmlBodyPartTrumbowygEditorSettingsDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<TrumbowygSettingsViewModel>("HtmlBodyPartTrumbowygSettings_Edit", model =>
        {
            var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartTrumbowygEditorSettings>();

            model.Options = settings.Options;
            model.InsertMediaWithUrl = settings.InsertMediaWithUrl;
        }).Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        if (contentTypePartDefinition.Editor() == "Trumbowyg")
        {
            var model = new TrumbowygSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            try
            {
                var options = model.Options.Trim();

                if (!options.StartsWith('{') || !options.EndsWith('}'))
                {
                    throw new Exception();
                }

                var parser = new Parser();

                var optionsScript = parser.ParseScript("var config = " + options);

                var settings = new HtmlBodyPartTrumbowygEditorSettings
                {
                    InsertMediaWithUrl = model.InsertMediaWithUrl,
                    Options = options
                };

                context.Builder.WithSettings(settings);
            }
            catch
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Options), S["The options are written in an incorrect format."]);
            }
        }

        return Edit(contentTypePartDefinition, context);
    }
}

using Acornima;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Markdown.Models;
using OrchardCore.Markdown.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Markdown.Settings;

public sealed class MarkdownBodyPartWysiwygEditorSettingsDriver : ContentTypePartDefinitionDisplayDriver<MarkdownBodyPart>
{
    internal readonly IStringLocalizer S;

    public MarkdownBodyPartWysiwygEditorSettingsDriver(IStringLocalizer<MarkdownBodyPartWysiwygEditorSettingsDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<MarkdownFieldWysiwygEditorSettingsViewModel>("MarkdownBodyPartWysiwygEditorSettings_Edit", model =>
        {
            var settings = contentTypePartDefinition.GetSettings<MarkdownBodyPartWysiwygEditorSettings>();

            model.Options = settings.Options;
        }).Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        if (contentTypePartDefinition.Editor() == "Wysiwyg")
        {
            var model = new MarkdownFieldWysiwygEditorSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            if (!string.IsNullOrWhiteSpace(model.Options))
            {
                try
                {
                    var options = model.Options.Trim();

                    if (!options.StartsWith('{') || !options.EndsWith('}'))
                    {
                        throw new ParseErrorException("The options must be a valid JavaScript object literal.");
                    }

                    var parser = new Parser();

                    parser.ParseScript("var config = " + options);

                    var settings = new MarkdownBodyPartWysiwygEditorSettings
                    {
                        Options = options,
                    };

                    context.Builder.WithSettings(settings);
                }
                catch (ParseErrorException)
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Options), S["The options are written in an incorrect format."]);
                }
            }
            else
            {
                context.Builder.WithSettings(new MarkdownBodyPartWysiwygEditorSettings());
            }
        }

        return Edit(contentTypePartDefinition, context);
    }
}

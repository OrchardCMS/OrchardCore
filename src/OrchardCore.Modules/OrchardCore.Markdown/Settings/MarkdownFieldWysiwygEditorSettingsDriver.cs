using Acornima;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Markdown.Settings;

public sealed class MarkdownFieldWysiwygEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MarkdownField>
{
    internal readonly IStringLocalizer S;

    public MarkdownFieldWysiwygEditorSettingsDriver(IStringLocalizer<MarkdownFieldWysiwygEditorSettingsDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<MarkdownFieldWysiwygEditorSettingsViewModel>("MarkdownFieldWysiwygEditorSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<MarkdownFieldWysiwygEditorSettings>();

            model.Options = settings.Options;
        }).Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (partFieldDefinition.Editor() == "Wysiwyg")
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
                        throw new Exception();
                    }

                    var parser = new Parser();

                    parser.ParseScript("var config = " + options);

                    var settings = new MarkdownFieldWysiwygEditorSettings
                    {
                        Options = options,
                    };

                    context.Builder.WithSettings(settings);
                }
                catch
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Options), S["The options are written in an incorrect format."]);
                }
            }
            else
            {
                context.Builder.WithSettings(new MarkdownFieldWysiwygEditorSettings());
            }
        }

        return Edit(partFieldDefinition, context);
    }
}

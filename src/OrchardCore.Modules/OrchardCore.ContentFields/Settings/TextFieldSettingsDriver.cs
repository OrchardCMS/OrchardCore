using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings;

public sealed partial class TextFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<TextField>
{
    private const string TextFieldColorEditor = "Color";

    internal readonly IStringLocalizer S;

    public TextFieldSettingsDriver(IStringLocalizer<TextFieldSettingsDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<TextFieldSettings>("TextFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<TextFieldSettings>();

            model.Hint = settings.Hint;
            model.Required = settings.Required;
            model.DefaultValue = settings.DefaultValue;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var contentPartFieldSettings = partFieldDefinition.GetSettings<ContentPartFieldSettings>();
        var model = new TextFieldSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        // Prevent accepting color with #XXX format, because the color editor will use black color all the time.
        if (contentPartFieldSettings.Editor == TextFieldColorEditor)
        {
            if (!HexColor().IsMatch(model.DefaultValue))
            {
                context.Updater.ModelState.AddModelError(Prefix, S["A value for {0} should be in '#XXXXXX' format.", partFieldDefinition.DisplayName()]);
            }
        }

        context.Builder.WithSettings(model);

        return Edit(partFieldDefinition, context);
    }

    [GeneratedRegex(@"^#(?:[0-9a-fA-F]{6})$")]
    private static partial Regex HexColor();
}

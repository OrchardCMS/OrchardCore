using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Settings;

public sealed class TextFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<TextField>
{
    private readonly ILiquidTemplateManager _templateManager;

    internal readonly IStringLocalizer S;

    public TextFieldSettingsDriver(
        ILiquidTemplateManager templateManager,
        IStringLocalizer<TextFieldSettingsDriver> stringLocalizer)
    {
        _templateManager = templateManager;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<TextFieldSettingsViewModel>("TextFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<TextFieldSettings>();

            model.Hint = settings.Hint;
            model.Required = settings.Required;
            model.DefaultValue = settings.DefaultValue;
            model.Type = settings.Type;
            model.Pattern = settings.Pattern;
            model.Types = new List<SelectListItem>
            {
                new(S["Editable"], nameof(FieldBehaviorType.Editable)),
                new(S["Generated Hidden"], nameof(FieldBehaviorType.GeneratedHidden)),
                new(S["Generated Disabled"], nameof(FieldBehaviorType.GeneratedDisabled)),
            };
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new TextFieldSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (model.Type == FieldBehaviorType.GeneratedHidden || model.Type == FieldBehaviorType.GeneratedDisabled)
        {
            if (string.IsNullOrWhiteSpace(model.Pattern))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Pattern), S["A pattern is required when using the selected behavior type."]);
            }
            else if (!_templateManager.Validate(model.Pattern, out var errors))
            {
                context.Updater.ModelState.AddModelError(nameof(model.Pattern), S["Pattern doesn't contain a valid Liquid expression. Details: {0}", string.Join(' ', errors)]);
            }
        }

        context.Builder.WithSettings(new TextFieldSettings
        {
            Hint = model.Hint,
            Required = model.Required,
            DefaultValue = model.DefaultValue,
            Type = model.Type,
            Pattern = model.Pattern,
        });

        return Edit(partFieldDefinition, context);
    }
}

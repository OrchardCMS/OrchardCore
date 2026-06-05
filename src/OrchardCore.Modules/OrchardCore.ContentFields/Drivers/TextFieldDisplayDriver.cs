using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Drivers;

public sealed class TextFieldDisplayDriver : ContentFieldDisplayDriver<TextField>
{
    internal readonly IStringLocalizer S;

    public TextFieldDisplayDriver(IStringLocalizer<TextFieldDisplayDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Display(TextField field, BuildFieldDisplayContext context)
    {
        return Initialize<DisplayTextFieldViewModel>(GetDisplayShapeType(context), model =>
        {
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        })
        .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
        .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
    }

    public override IDisplayResult Edit(TextField field, BuildFieldEditorContext context)
    {
        return Initialize<EditTextFieldViewModel>(GetEditorShapeType(context), model =>
        {
            var settings = context.PartFieldDefinition.GetSettings<TextFieldSettings>();
            model.Text = context.IsNew && field.Text == null ? settings.DefaultValue : field.Text;
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(TextField field, UpdateFieldEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(field, Prefix, f => f.Text);
        var settings = context.PartFieldDefinition.GetSettings<TextFieldSettings>();

        if (settings.Required && string.IsNullOrWhiteSpace(field.Text))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(field.Text), S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
        }

        var length = field.Text?.Length ?? 0;

        if (settings.MinLength > 0 && length > 0 && length < settings.MinLength)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(field.Text), S["{0} must be at least {1} characters long.", context.PartFieldDefinition.DisplayName(), settings.MinLength]);
        }

        if (settings.MaxLength > 0 && length > settings.MaxLength)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(field.Text), S["{0} can't be longer than {1} characters.", context.PartFieldDefinition.DisplayName(), settings.MaxLength]);
        }

        return Edit(field, context);
    }
}

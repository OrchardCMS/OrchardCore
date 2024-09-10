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

public sealed class DateFieldDisplayDriver : ContentFieldDisplayDriver<DateField>
{
    internal readonly IStringLocalizer S;

    public DateFieldDisplayDriver(IStringLocalizer<DateFieldDisplayDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Display(DateField field, BuildFieldDisplayContext context)
    {
        return Initialize<DisplayDateFieldViewModel>(GetDisplayShapeType(context), model =>
        {
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        })
        .Location("Detail", "Content")
        .Location("Summary", "Content");
    }

    public override IDisplayResult Edit(DateField field, BuildFieldEditorContext context)
    {
        return Initialize<EditDateFieldViewModel>(GetEditorShapeType(context), model =>
        {
            model.Value = field.Value;
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(DateField field, UpdateFieldEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(field, Prefix, f => f.Value);
        var settings = context.PartFieldDefinition.GetSettings<DateFieldSettings>();

        if (settings.Required && field.Value == null)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(field.Value), S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
        }

        return Edit(field, context);
    }
}

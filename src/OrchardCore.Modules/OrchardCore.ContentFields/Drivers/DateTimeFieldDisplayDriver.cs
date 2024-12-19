using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Modules;
using OrchardCore.Mvc.ModelBinding;


namespace OrchardCore.ContentFields.Drivers;

public sealed class DateTimeFieldDisplayDriver : ContentFieldDisplayDriver<DateTimeField>
{
    private readonly ILocalClock _localClock;

    internal readonly IStringLocalizer S;

    public DateTimeFieldDisplayDriver(
        ILocalClock localClock,
        IStringLocalizer<DateTimeFieldDisplayDriver> localizer)
    {
        _localClock = localClock;
        S = localizer;
    }

    public override IDisplayResult Display(DateTimeField field, BuildFieldDisplayContext context)
    {
        return Initialize<DisplayDateTimeFieldViewModel>(GetDisplayShapeType(context), async model =>
        {
            model.LocalDateTime = field.Value == null ? null : (await _localClock.ConvertToLocalAsync(field.Value.Value)).DateTime;
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        })
        .Location("Detail", "Content")
        .Location("Summary", "Content");
    }

    public override IDisplayResult Edit(DateTimeField field, BuildFieldEditorContext context)
    {
        return Initialize<EditDateTimeFieldViewModel>(GetEditorShapeType(context), async model =>
        {
            model.LocalDateTime = field.Value == null ? null : (await _localClock.ConvertToLocalAsync(field.Value.Value)).DateTime;
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(DateTimeField field, UpdateFieldEditorContext context)
    {
        var model = new EditDateTimeFieldViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, f => f.LocalDateTime);
        var settings = context.PartFieldDefinition.GetSettings<DateTimeFieldSettings>();

        if (settings.Required && model.LocalDateTime == null)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.LocalDateTime), S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
        }
        else
        {
            if (model.LocalDateTime == null)
            {
                field.Value = null;
            }
            else
            {
                field.Value = await _localClock.ConvertToUtcAsync(model.LocalDateTime.Value);
            }
        }

        return Edit(field, context);
    }
}

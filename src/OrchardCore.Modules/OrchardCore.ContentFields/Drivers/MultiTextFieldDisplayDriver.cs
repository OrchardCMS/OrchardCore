using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Fields;

public sealed class MultiTextFieldDisplayDriver : ContentFieldDisplayDriver<MultiTextField>
{
    internal readonly IStringLocalizer S;

    public MultiTextFieldDisplayDriver(IStringLocalizer<MultiTextFieldDisplayDriver> localizer)
    {
        S = localizer;
    }

    public override IDisplayResult Display(MultiTextField field, BuildFieldDisplayContext context)
    {
        return Initialize<DisplayMultiTextFieldViewModel>(GetDisplayShapeType(context), model =>
        {
            var settings = context.PartFieldDefinition.GetSettings<MultiTextFieldSettings>();

            model.Values = settings.Options.Where(o => field.Values?.Contains(o.Value) == true).Select(o => o.Value).ToArray();
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        })
        .Location("Detail", "Content")
        .Location("Summary", "Content");
    }

    public override IDisplayResult Edit(MultiTextField field, BuildFieldEditorContext context)
    {
        return Initialize<EditMultiTextFieldViewModel>(GetEditorShapeType(context), model =>
        {
            if (context.IsNew)
            {
                var settings = context.PartFieldDefinition.GetSettings<MultiTextFieldSettings>();
                model.Values = settings.Options.Where(o => o.Default).Select(o => o.Value).ToArray();
            }
            else
            {
                model.Values = field.Values;
            }
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(MultiTextField field, UpdateFieldEditorContext context)
    {
        var viewModel = new EditMultiTextFieldViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        field.Values = viewModel.Values;

        var settings = context.PartFieldDefinition.GetSettings<MultiTextFieldSettings>();
        if (settings.Required && viewModel.Values.Length == 0)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(field.Values), S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
        }

        return Edit(field, context);
    }
}

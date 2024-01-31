using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.ContentFields.Drivers
{
    public class TimeFieldDisplayDriver : ContentFieldDisplayDriver<TimeField>
    {
        protected readonly IStringLocalizer S;

        public TimeFieldDisplayDriver(IStringLocalizer<TimeFieldDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Display(TimeField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayTimeFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Detail", "Content")
            .Location("Summary", "Content");
        }

        public override IDisplayResult Edit(TimeField field, BuildFieldEditorContext context)
        {
            return Initialize<EditTimeFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Value = field.Value;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(TimeField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            if (await updater.TryUpdateModelAsync(field, Prefix, f => f.Value))
            {
                var settings = context.PartFieldDefinition.GetSettings<TimeFieldSettings>();
                if (settings.Required && field.Value == null)
                {
                    updater.ModelState.AddModelError(Prefix, nameof(field.Value), S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
            }

            return Edit(field, context);
        }
    }
}

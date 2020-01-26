using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Fields
{
    public class MultiValueFieldDisplayDriver : ContentFieldDisplayDriver<MultiValueField>
    {
        private readonly IStringLocalizer S;

        public MultiValueFieldDisplayDriver(IStringLocalizer<MultiValueFieldDisplayDriver> localizer)
        {
            S = localizer;
        }

        public override IDisplayResult Display(MultiValueField field, BuildFieldDisplayContext context)
        {
            return Initialize<DisplayMultiValueFieldViewModel>(GetDisplayShapeType(context), model =>
            {
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            })
            .Location("Content")
            .Location("SummaryAdmin", "");
        }

        public override IDisplayResult Edit(MultiValueField field, BuildFieldEditorContext context)
        {
            return Initialize<EditMultiValueFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Values = field.Values;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MultiValueField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            if (await updater.TryUpdateModelAsync(field, Prefix, f => f.Values))
            {
                var settings = context.PartFieldDefinition.GetSettings<MultiValueFieldSettings>();
                if (settings.Required && !field.Values.Any())
                {
                    updater.ModelState.AddModelError(Prefix, S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
            }

            return Edit(field, context);
        }
    }
}
